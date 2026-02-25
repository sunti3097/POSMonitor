# POS Monitor (ระบบเฝ้าระวังเครื่อง POS)

ระบบนี้แยกเป็น 3 ส่วนหลัก

1. **Server (ASP.NET Core Web API + Dashboard)**
   - จัดเก็บสถานะ, หัวใจ (Heartbeat), คำสั่งรีโมต และกำหนดกลุ่ม/ช่วงเวลาแจ้งเตือน
   - ส่งการแจ้งเตือน Email / Microsoft Teams เมื่อเครื่องใด offline
   - เปิด REST API ให้ Agent รายงานสถานะ พร้อมรับคำสั่งตอบกลับ
2. **Agent (Windows Service)**
   - ติดตั้งบนแต่ละ POS (Windows) เพื่อตรวจสอบ Hardware, Network, Service และ Process เฉพาะ (GeniuzCli, GeniuzSync.exe, window_genuiz.exe)
   - มีคิวภายใน (SQL Express) สำหรับเก็บ Heartbeat หาก offline แล้วส่งซ้ำอัตโนมัติเมื่อ online
   - รับและประมวลผลคำสั่งระยะไกล (เช่น Restart process/service, Update config)
3. **Shared**
   - เก็บ DTO/Contracts/Enums ที่ใช้ร่วมกันระหว่าง Server และ Agent

## คุณสมบัติเด่น
- .NET 8 ตลอดทั้ง solution (รองรับ IIS Hosting)
- Entity Framework Core + SQL Server (ฝั่ง Server) และ SQL Express LocalDB (ฝั่ง Agent)
- Scheduling แจ้งเตือนตามกลุ่ม POS และช่วงเวลา (Notification Window)
- API Key Authentication สำหรับ Agent (`X-Agent-Key`)
- รองรับการสั่งงานจาก Dashboard เช่น Restart Process, Update Config

## โครงสร้างโฟลเดอร์
```
c:\POSMonitor
├── src
│   ├── Server         # ASP.NET Core Web API + Dashboard (กำลังพัฒนา UI)
│   ├── Agent          # Worker Service สำหรับติดตั้งบน POS
│   └── Shared         # DTO/Contracts/Enums ที่แชร์กัน
└── README.md          # เอกสารนี้
```

## ความต้องการระบบ
- Windows Server / Windows 10+ สำหรับ run Server และ IIS
- .NET 8 SDK (8.0.418+) และ Hosting bundle เมื่อ deploy บน IIS
- SQL Server 2019+ (หรือ Azure SQL) สำหรับ Backend
- แต่ละเครื่อง POS ต้องติดตั้ง SQL Express LocalDB (หรือ SQL Express instance) สำหรับเก็บ queue ชั่วคราว

## การตั้งค่าฝั่ง Server
1. คัดลอก `src/Server/appsettings.Template.json` เป็น `appsettings.json` แล้วแก้ไขค่าตามสภาพแวดล้อม
   - `ConnectionStrings:Default`
   - `Monitoring` (threshold ต่าง ๆ)
   - `Notifications.Email` และ `Notifications.Teams`
   - `AgentAuthentication.ApiKey` (ต้องตรงกับ Agent)
2. รัน migration เพื่อติดตั้งฐานข้อมูล (ครั้งแรก)
   ```powershell
   cd c:\POSMonitor\src\Server
   dotnet ef database update
   ```
3. รันทดสอบ
   ```powershell
   dotnet run
   # หรือ publish แล้ว deploy IIS
   dotnet publish -c Release
   ```
4. (ถ้าใช้ IIS) สร้าง App Pool (.NET CLR: No Managed Code) แล้วชี้ไปที่โฟลเดอร์ `publish` จากข้อ 3

## การตั้งค่าฝั่ง Agent
1. คัดลอก `src/Agent/appsettings.Template.json` เป็น `appsettings.json` แล้วเติมรายละเอียดเครื่อง POS
   - `Agent.DeviceId` (GUID ไม่ซ้ำ)
   - `Agent.ApiBaseUrl` และ `Agent.ApiKey`
   - `Agent.SqlExpressConnectionString` สำหรับ queue
   - `MonitoringTargets` (บริการ/โปรเซสที่ต้องเฝ้าระวัง)
2. สร้าง Windows Service
   ```powershell
   cd c:\POSMonitor\src\Agent
   dotnet publish -c Release
   # โฟลเดอร์ผลลัพธ์: bin\Release\net8.0\publish

   # ลงทะเบียน service (รันใน PowerShell ที่มีสิทธิ์สูง)
   sc create POSMonitorAgent binPath= "C:\POSMonitor\src\Agent\bin\Release\net8.0\publish\POSMonitor.Agent.exe"
   sc start POSMonitorAgent
   ```
3. Agent จะบันทึก Heartbeat ลง SQL Express หากส่งไม่ได้ และ retry อัตโนมัติเมื่อ network คืนความปกติ

### สคริปต์ช่วย deploy
- `scripts/Deploy-Server.ps1` : build/publish Server + ตั้งค่า IIS site/app pool อัตโนมัติ (ปรับ parameter ให้ตรงกับ environment)
- `scripts/Deploy-Agent.ps1` : publish Agent + ติดตั้ง/อัปเดต Windows Service


## API Authentication
- ทุกคำขอจาก Agent ไป Server ต้องมี header `X-Agent-Key`
- สามารถใช้ Reverse Proxy หรือ IP whitelist เพิ่มเติมได้ หากต้องการจำกัด network

## Notification Channels
- Email: ใช้ SMTP relay ภายในองค์กร (ต้องเพิ่มการตั้งค่า Host/Port/Credential ในขั้นต่อไป)
- Microsoft Teams: ใช้ Incoming Webhook URL ต่อกลุ่ม
- สามารถปิดเปิดช่องทางผ่าน config ได้

## Roadmap / งานที่กำลังดำเนินการ
1. สร้าง Dashboard UI (Role-based) สำหรับมอนิเตอร์สถานะ, จัดการกลุ่ม, ส่งคำสั่ง (กำลังพัฒนา)
2. เพิ่ม README Section ภาษาอังกฤษ + ตัวอย่าง config file (template)
3. เพิ่ม script อัตโนมัติสำหรับ deploy IIS + ติดตั้ง Agent service
4. ตั้งค่า Git repository `https://github.com/sunti3097/POSMonitor.git` พร้อมเอกสาร deploy

## การสำรองลง GitHub
เมื่อแก้ไขครบทุกส่วน ให้ทำดังนี้
```powershell
cd c:\POSMonitor
git init
git remote add origin https://github.com/sunti3097/POSMonitor.git
git add .
git commit -m "Initial POS Monitor implementation"
git push -u origin main
```
> หมายเหตุ: หาก repo มีอยู่แล้ว ให้ pull branch ปัจจุบันก่อน แล้วแก้ merge ตาม workflow ที่ทีมกำหนด

---
หากต้องการรายละเอียดเพิ่มเติม (sequence diagram, identity setup, dashboard spec) โปรดแจ้งได้เลย
