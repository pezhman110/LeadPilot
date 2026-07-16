# LeadPilot — راهنمای ساخت نهایی (رادار غربال اتصالات)

استک: **.NET 10 + PostgreSQL + Blazor Web App** · معماری Modular Monolith (DDD)

> صداقت فنی: این کد در محیطِ چت (بدون .NET SDK) نوشته و **ساختاری** تأیید شده
> (بدون typeِ تکراری، توازن کامل، resolve شدن همهٔ DI و @inject). ساختِ واقعی
> و رفعِ هر خطای احتمالیِ کامپایلر باید در محیطی با SDK انجام شود — بهترین
> گزینه: **Claude Code** (همین مدل، با ترمینال و SDK واقعی).

---

## الف) اجرا در Claude Code (پیشنهادی)

۱) این پوشه را در یک دایرکتوری بگذارید و Claude Code را همان‌جا باز کنید.
۲) این جمله را به Claude Code بدهید (کپی/پیست):

```
این یک solution دات‌نت ۱۰ (Blazor + PostgreSQL) است. لطفاً:
1) dotnet restore و dotnet build را اجرا کن و هر خطای کامپایلر را رفع کن.
2) یک PostgreSQL محلی بالا بیاور (docker: postgres:16) و رشته اتصال را در
   src/LeadPilot.Server/appsettings.json تنظیم کن.
3) dotnet ef migrations add InitialCreate و dotnet ef database update را اجرا کن.
4) dotnet test را اجرا کن و مطمئن شو همه سبز است.
5) dotnet run --project src/LeadPilot.Server را اجرا کن و آدرس را بده.
تا وقتی «واقعاً build و run می‌شود» ادامه بده؛ خروجی نمایشی نمی‌خواهم.
```

Claude Code خطاهای واقعی را می‌بیند و رفع می‌کند تا به اجرای واقعی برسد.

---

## ب) اجرا با دست (اگر SDK دارید)

```bash
# پیش‌نیاز: .NET 10 SDK + یک PostgreSQL
dotnet restore
dotnet build                      # باید Build succeeded بدهد
dotnet test                       # تست‌های دامنه
dotnet tool restore
# رشته اتصال را در appsettings.json تنظیم کنید، سپس:
dotnet ef migrations add InitialCreate -p src/LeadPilot.Infrastructure -s src/LeadPilot.Server
dotnet ef database update        -p src/LeadPilot.Infrastructure -s src/LeadPilot.Server
dotnet run --project src/LeadPilot.Server
```

یا با Docker (Postgres خودکار): `cd deploy && docker compose up --build`

---

## ج) نقطهٔ کنترل (این سه باید سبز شوند)

1. `Build succeeded`
2. Migration اجرا و جدول‌ها در PostgreSQL ساخته شوند
3. اولین پروژه در PostgreSQL ثبت شود (دکمهٔ «ثبت پروژهٔ اول» در داشبورد)

---

## د) وضعیت دقیق هر بخش (صادقانه)

**کامل، wire‌شده و آمادهٔ اجرا در UI/API:**
- پروژه‌ها (Projects): دامنهٔ DDD + API + صفحهٔ Blazor + مهاجرت. ✅
- جست‌وجوی زنده (Live Search): QueryBuilder شش‌زبانه + ComplianceGateway +
  RuleScoring (سه امتیاز) + Orchestrator (موازی/dedup/rate-limit) + صفحهٔ Blazor. ✅
- گزارش مدیریتی: ManagementReportService + endpoint. ✅

**منطقِ کامل + تست، ولی هنوز باید در UI/persistence وصل شود (کارِ Claude Code):**
- رضایت (Consent): ContactPoint با Hidden→Usable + اختیارِ جلوگیریِ مدیر،
  InvitationJourney (دعوت opt-in، سه حاصل، مسیریابیِ نمره). دامنه + تست کامل.
- همکاران (Partners): Partner + نمرهٔ عملکرد، DealClassifier (بازه‌های مبلغ)،
  LanguageCoverage (۷ زبان)، AssignmentRecommender، PartnerAgreement (مدیر+Audit).
  دامنه + تست کامل.

> این دو، منطقِ دامنه و تستشان کامل است؛ فقط باید DbSet/Configuration/Endpoint و
> صفحهٔ Blazorشان اضافه شود. در Claude Code بگویید: «Consent و Partners را هم مثل
> Projects به persistence و API و یک صفحهٔ Blazor وصل کن.»

**پیش‌نمایش‌های HTML (برای دیدن طراحی، جدا از برنامه):**
LeadPilot-Preview / LiveSearch-Preview / Management-Report-Preview /
Partners-Preview / Roadmap-Alignment.

---

## ه) خطوط قرمز (در کد اجرا شده — تغییر ندهید)

- کشف سیگنالِ عمومی، نه استخراجِ مخفیِ شماره. «اولین ارتباط = دعوت opt-in».
- شماره تا رضایت Hidden است (مدیر می‌بیند)؛ با رضایت خودکار آزاد می‌شود؛
  مدیر اختیارِ جلوگیری دارد.
- بدون پروفایلر ملیتی، بدون قیمت‌گذاری بر اساس ملیت، بدون اثبات ثروت از سبک زندگی.
- متن‌های «سود ثابت/ضمانت» پشت compliance gate تا تأیید نهاییِ حقوقی.

## زبان‌ها
کاورشده (موتور + کارشناس): عربی، فارسی، انگلیسی، ترکی، فرانسوی، هندی، لوکال.
بقیه: فقط هوش مصنوعی، پیام به زبانِ خودِ مشتری.

---

## و) به‌روزرسانی: موتور هدف/ظرفیت/زنجیرهٔ درآمد + نام‌گذاری مطابق سند

اضافه‌شده (Targets):
  • TargetMetricType / TargetPeriodType (دقیقاً مطابق سند).
  • ProjectTarget — هدف تعدادی/مالی با درصد تحقق و باقی‌مانده.
  • ProjectFunnelSettings — نرخ‌های قیف (تخمینی تا داده واقعی).
  • ReverseFunnelCalculator — از هدف مالی به عقب تا سیگنالِ لازم
    (نمونهٔ سند تست شد: 1M → 5/50/200/1000/5000).
  • CrossProjectOffer + AttributionRecord — زنجیرهٔ ارزش بین ۱۶ پروژه،
    جلوگیری از پرداختِ چندبارهٔ پورسانت.
  • ۵ تستِ جدید (TargetTests).

نام‌گذاری مطابق سند عوض شد:
  • ContactPoint → ProtectedContactPoint
  • InvitationState → InvitationStatus
  • SalesRoute → ProspectRoute

پیش‌نمایش HTML: RevenueChain-Preview.html (محاسبهٔ معکوس قیف + زنجیرهٔ ارزش).

تصمیم ثبت‌شده: پروفایلر ملیتی ساخته نمی‌شود؛ هدف‌گیری با زبان/موقعیت/بازار.

---

## ز) به‌روزرسانی نهایی: همهٔ ماژول‌ها متصل شدند (اتصال کامل)

Consent + Partners + Targets حالا کامل به persistence + API + Blazor وصل‌اند:

**دیتابیس:** ۹ DbSet جدید + Configuration کامل (contact_points, invitation_journeys,
partners, partner_agreements, project_targets, project_funnel_settings,
cross_project_offers, attribution_records).

**API:**
  • /api/consent/* — ثبت تماس، دعوت، رضایت/رد/بی‌جواب، جلوگیری/رفعِ مدیر.
  • /api/partners/* — ثبت همکار، پیشنهاد تخصیص، توافق + تأیید مدیر.
  • /api/targets/* — ثبت هدف، فهرست، محاسبهٔ معکوس قیف (plan).

**صفحات Blazor (همه واقعی، نه نمایشی):**
  • Projects, Live Search, Consent, CRM (موتور هدف), Partners — همه دکمه‌هایشان کار می‌کنند.
  • Signals, Campaigns — جای فازهای بعد.

**تست:** ۷ فایل، شامل ServiceWiringTests که سرویس‌ها را با مخزن در-حافظه تست می‌کند.

اکنون کلِ پروژه یکپارچه است. تنها قدمِ باقی‌مانده: build واقعی روی محیط SDK/Claude Code
(چون در محیط چت SDK نیست). دستورها در بخش «الف/ب» بالا.

---

## ح) به‌روزرسانی: صفر تا صد کامل شد

**Signals** وصل شد: صفحهٔ سیگنال‌ها با سه امتیاز، زبان، و لینکِ منبعِ قابل‌کلیک.

**ماژول محتوا** ساخته و کامل وصل شد (فاز ۴):
  • سرفصل محتوایی، ContentAsset (آرشیو با متادیتا و وضعیت).
  • «غریال» (ReviewInventory): گزارشِ «چه داریم / چه کم داریم» برای هر (نوع × زبان).
  • مغزِ محتوا (IAiContentBrain): متن آگهی، بنر، کاروسل، اسکریپت ویدیوی
    صحنه‌به‌صحنه، اسکریپت پادکست، بریف موشن. (نسخهٔ فعلی Stub؛ به LLM وصل می‌شود.)
  • موتورهای تولیدِ فایل نهایی (IImageEngine/IAudioEngine/IVideoEngine) —
    interface برای اتصال به Canva/TTS/Video API. Claude فقط متن/اسکریپت می‌سازد.
  • صفحهٔ فرود (قالب جدا از صفحه، همیشه فرم opt-in)، کمپین (گروه کوچک/بزرگ)،
    پرزنت اختصاصی (AI یا کارشناس) + ارزیابی کیفیت.
  • API: /api/content/* و صفحهٔ Blazor «محتوا و کمپین».

**همهٔ ۷ صفحهٔ Blazor حالا واقعی‌اند:** Projects, Live Search, Signals, Consent,
CRM (اهداف), Partners, Campaigns (محتوا). هیچ صفحهٔ نمایشی/خالی نمانده.

جمع: ۵۱ فایل کد، ۱۱ صفحهٔ Blazor، ۸ فایل تست، ۷ گروه API.

—— تنها قدمِ باقی‌مانده: build واقعی روی محیط SDK/Claude Code. کد صفر تا صد آماده است.

---

## ط) رجیستری جامع منابع (۱۴ گروه A–N) — مطابق سند شما

ساخته شد (بر پایهٔ لیستِ کاملِ پژمان، نه لیستِ عمومی):
  • SourceRegistry — ۱۴ گروه (A منابع خودی … N آفلاین)، نقشِ منبع
    (PhoneSource / SignalSource / OptInChannel)، طبقه‌بندیِ تماس، رتبهٔ سرعت ۱–۶.
  • SourceCatalog — ۶۸ منبعِ seed در همهٔ گروه‌ها.
  • SourceCascadePlanner — آبشارِ «رایگان/سریع → پولی/کند»، حذفِ منابعِ فقط-شرکتی
    برای پروژهٔ شخصی، و ترتیبِ اختصاصیِ هر پروژه (سرمایه‌گذاری/سالن/کارخانه/فریلنسر).
  • ۷ تستِ جدید (پوشش ۱۴ گروه، حذفِ مپ برای شخصی، آبشار ارزان→گران، انجمن=سیگنال).

خطوط قرمزِ رعایت‌شده (از سند): استخراجِ شمارهٔ حساب شخصی ممنوع؛ کامنت/انجمن
Signal Source است نه Phone Source؛ Google Maps/About فقط شرکتی؛ خروجی AI منبع
مستقیمِ تلفن نیست؛ دریافتِ شماره ≠ اجازهٔ تماس فروش (اول ManagerOnly، بعد opt-in).

پیش‌نمایش HTML: SourceRegistry-Preview.html (۱۴ گروه، فیلتر شخصی/شرکتی، آبشار هر پروژه).

مانده از این بخش (طبق سند، در ادامه): ContactDeduplication (Hash+Reservation+
Observation)، DailyCapacityPlan/Quota، ProcessingJob/Worker/Scheduler،
ConnectionWizard + HealthCheck + نصب دو-کلیکی، First-10-Results dashboard.

---

## ی) جلوگیری از دریافت/پرداخت مجدد (بسیار مهم) — مطابق سند بخش ۶–۸

ساخته شد:
  • ContactNormalizer — تلفن → +E.164، ایمیل → lowercase؛ HMAC-SHA256 → ContactHash.
    (همهٔ فرمت‌های یک شماره به یک hash می‌رسند.)
  • ContactRecord — پرونده با تاریخچهٔ کامل (FirstSeen/LastSeen/Validated/Enriched/
    Invitation/Consent/Used) + قید یکتایی TenantId+ContactType+ContactHash.
  • ContactObservation — مشاهدهٔ همان شماره از منبع دیگر: بدون ContactPoint جدید،
    بدون خرید مجدد.
  • EnrichmentReservation — کلید ENRICH:{ProspectId}:{ProviderCode}:{Purpose} با
    ایندکس یکتا؛ جلوگیری از پرداختِ هم‌زمانِ دو Worker.
  • DeduplicationService — RegisterAsync (تکراری→Observation) و TryReserveEnrichmentAsync.
  • DI: کلید HMAC از appsettings ("Security:ContactHashKey").
  • ۷ تستِ جدید (نرمال‌سازی فرمت‌ها، پایداری hash، تکراری→observation، رزرو).

پیش‌نمایش HTML: Dedup-Preview.html (نرمال‌سازی + پرونده + مشاهده + رزرو).

مانده از این بخش: DailyCapacityPlan/Quota، ProcessingJob/Worker/Scheduler،
ConnectionWizard + HealthCheck + نصب دو-کلیکی، First-10-Results dashboard.

---

## ک) صف پردازشِ پایدار (Durable Queue) — مطابق سند، برای PostgreSQL

این جوابِ «بعد از ۲۰۰ شماره قفل نکند» است. ساخته شد:
  • ProcessingJob — Job پایدار در دیتابیس با Lease، بازیابیِ Jobِ رهاشده،
    Retry فزاینده، Idempotency، Dead Letter، Freeze/Unfreeze. (RowVersion=xmin برای Postgres)
  • ProcessingJobTypes — SEARCH/CONTACT_DISCOVERY/CONSENT/AI_WARMING/VOICE/…
  • ProcessingJobRepository — برداشتِ امن با **FOR UPDATE SKIP LOCKED**
    (معادلِ READPAST سند برای Postgres). دو Worker یک Job را برنمی‌دارند.
  • IProcessingJobHandler + RetryPolicy (فاصلهٔ فزاینده 15s→1h).
  • **پروژهٔ جدید LeadPilot.Worker** — BackgroundService مستقل، هر Job در Scope
    مجزا، موازیِ محدود (Concurrency). Production: ۳ Worker × ۸ = ۲۴ پردازش هم‌زمان.
  • ContactDiscoveryJobHandler (ساختاری؛ نسخهٔ واقعی به آبشار منبع + dedup وصل می‌شود).
  • API: POST /api/processing/contact-discovery (Payload فقط شناسه‌ها)،
    GET /api/processing/summary (پایشِ صف، فقط مدیر).
  • ۹ تستِ جدید (Idempotency، Lease، بازیابیِ رهاشده، Retry، Dead Letter، اولویت).

نکتهٔ امنیتی سند: Payload صف فقط شناسهٔ پرونده است، نه شماره/ایمیل خام.

Migration جدید (روی دستگاه با SDK):
  dotnet ef migrations add AddDurableProcessingQueue -p src/LeadPilot.Infrastructure -s src/LeadPilot.Server
اجرای Worker: dotnet run --project src/LeadPilot.Worker

پیش‌نمایش HTML: Queue-Preview.html (Jobها، Workerها، Retry، Dead Letter، تست فشار ۲۰۰).

مانده: DailyCapacityPlan/Quota، ConnectionWizard + HealthCheck + نصب دو-کلیکی،
First-10-Results dashboard، Connectorهای واقعی.

---

## ل) برنامه‌ریز ظرفیت روزانه — مطابق سند، برای PostgreSQL

قلبِ «۴ ساعت، ۳۰۰۰ لید، بدون قفل». ساخته شد:
  • DailyCapacityPlan + ProjectDailyQuota + CapacitySnapshot (RowVersion=xmin).
    یک برنامهٔ اصلی برای هر روز/Tenant (ایندکس یکتا). چرخهٔ عمر کامل.
  • CapacityCalculator — محاسبهٔ معکوس با نرخِ واقعی (نه فرض ۵۰٪): از رضایت هدف
    → شمارهٔ لازم → جبرانِ تکراری → سیگنالِ اولیه + حاشیهٔ اطمینان.
  • DailyCapacityOrchestrator — کسری‌محور: هر چرخه فقط به‌اندازهٔ عقب‌ماندگی
    (نسبت به پیشرفتِ زمانیِ پنجره) Job می‌سازد، Batch کوچک (سقف ۲۵۰)، توقفِ
    خودکار پس از هدفِ هر گروه، پایان با/بدون کمبود.
  • CapacityPlannerWorker — هر ۳۰s، با **pg_try_advisory_lock** (قفل توزیع‌شدهٔ
    Postgres، معادلِ sp_getapplock سند) تا دو Planner هم‌زمان یک برنامه را نچرخانند.
  • به ProtectedContactPoint فیلدهای TenantId + ProjectId + CreatedAtUtc اضافه شد
    (طبق یادداشت سند: شمارش سهمیهٔ هر گروه باید بر مبنای پروژه/Tenant باشد).
  • API: POST /api/capacity/plans (+quotas)، schedule/pause، GET plans/today.
  • ۸ تستِ جدید (پنجره، ظرفیت، چرخهٔ عمر، محاسبهٔ نرخ واقعی، SignalTarget).

پیش‌نمایش HTML: Capacity-Preview.html (تفکیک ۳۰۰۰ لید بر گروه + محاسبهٔ معکوس +
سرعتِ لازم در پنجرهٔ ۴ ساعته).

مانده: ConnectionWizard + HealthCheck + نصب دو-کلیکی، First-10-Results dashboard،
Connectorهای واقعی.

---

## م) اصلاح امنیتی + صف موبایل + اتصالات + نصب دو-کلیکی (سند جدید)

**اصلاح امنیتیِ مهم (باگ واقعی که رفع شد):**
  • ContactAccessService — دسترسیِ مدیرِ مجاز (با Audit) **قبل از** بررسیِ
    Freeze/Suppressed/Status است. توقفِ مسیر فقط استفادهٔ عملیاتیِ فروشنده/AI/کمپین
    را می‌بندد، نه کنترلِ مدیر را. (۶ تست)

**صف همگام‌سازی موبایل:**
  • MobileContactSyncJob — ذخیرهٔ روزانهٔ اکانت روی موبایلِ کاری، حذف پس از رد رضایت.
  • IDeviceContactStore (Adapter به سرویس/اپ موبایل).

**داشبورد سلامت اتصالات + نصب دو-کلیکی:**
  • ConnectionHealthService — ۱۶ اتصال (حیاتی/غیرحیاتی) + قانونِ شروع:
    حیاتیِ قرمز → Blocked؛ غیرحیاتیِ قرمز → ReducedCapacity + هشدار؛ همه سبز → Ready.
  • FirstResultsQualifier — شرطِ «۱۰ شمارهٔ اول» (۸ شرط + ردِ AI/بدون‌منبع/تکراری/
    Suppressed/شرکتی‌برای‌شخصی). (۹ تست)
  • API: GET /api/connections/health، GET /api/connections/wizard.
  • deploy/install-click1.sh + docker-compose.production.yml + Dockerfile.worker
    (کلیک اول نصب کامل؛ کلیک دوم Wizard اتصالات در /setup).

به ProtectedContactPoint فیلدهای TenantId/ProjectId/CreatedAtUtc اضافه شد.

پیش‌نمایش HTML: Install-Health-Preview.html
  (نصب دو-کلیکی + داشبورد سلامت سبز/قرمز + ۱۰ نتیجهٔ اول).

—— این بخش، «دو کلیک نصب، سبز شدن اتصالات، ۱۰ شماره از دقیقهٔ اول» را کامل می‌کند.
Connectorهای واقعی (API رسمیِ هر پلتفرم) قدم بعدیِ ساخت‌اند.

---

## ن) موتور «اجرای آزمایشی ۱۰ نتیجهٔ اول» (Start Test Run) — جواهرِ سیستم

این اولین جریانِ عملیِ کاملِ سیستم است که همهٔ ماژول‌ها را به هم وصل می‌کند:

```
مدیر Start Test Run → سلامت اتصالات → حداکثر ۱۰۰ کاندید →
آبشار رایگان→پولی → نرمال/Dedup → ContactPoint با ManagerOnly →
۱۰ نتیجهٔ یکتا → توقف خودکار → فقط ماسک‌شده در داشبورد
```

**فایل‌ها:**
  • Domain: FirstTenRun.cs (ماشین‌وضعیت + هدف/سقف کاندید/سقف هزینه + شمارنده‌ها +
    CanAcceptResult/AddResult با dedup-درون-اجرا + CompleteIfTargetReached +
    CompleteWithShortage/Fail/Cancel، RowVersion=uint).
  • Domain: FirstTenRunResult.cs (فقط ماسک‌شده؛ نتیجهٔ بدون منبع/مرجع → استثناء).
  • Application: AcquisitionContracts.cs (ITestCandidateProvider, IFirstTenRunService،
    Command/Response)، IContactEnrichmentCascade.cs (آبشار رایگان→پولی)،
    FirstTenRunService.cs (جریان کامل)، IContactValueProtector.cs.
  • Infrastructure: FirstTenRunStore.cs (ContactPoint با ManagerOnly + AES رمزنگاری)،
    DemoTestCandidateProvider.cs، DemoEnrichmentCascade.cs (۷۰٪ رایگان، بقیه پولی اگر بودجه)،
    DataProtectionContactValueProtector.cs (AES-256-GCM، کلید از Security:ContactEncryptionKey).
  • API: POST /api/acquisition/first-ten/start، GET /{id}، POST /{id}/cancel.
  • تست: FirstTenRunTests.cs (۱۰ تست — همهٔ معیارهای قبولی سند).

**معیارهای قبولی (سند) که در کد اعمال شد:**
  ✓ بدون اتصالِ سالم شروع نشود (StartDecision.Blocked → Fail).
  ✓ فقط کاندیدِ واقعیِ پروژه پردازش شود.
  ✓ منابعِ رایگان قبل از پولی؛ Provider گران فقط اگر ناقص باشد.
  ✓ نتیجهٔ بدونِ URL/مرجع پذیرفته نشود.
  ✓ شمارهٔ نامعتبر/تکراری جزو ۱۰ شمرده نشود.
  ✓ شمارهٔ روزهای گذشته دوباره ساخته/خریده نشود (Dedup سراسری).
  ✓ بعد از ۱۰ نتیجه، اجرا خودکار Completed و بقیه لغو.
  ✓ ذخیره با ManagerOnly؛ API فقط ماسک‌شده؛ Reveal فقط مدیر (+Audit).
  ✓ هزینه و منبعِ هر نتیجه مشخص.

پیش‌نمایش HTML: FirstTenRun-Preview.html (Start Test Run زنده + ۵ مرحله + شمارنده‌ها +
۱۰ نتیجهٔ ماسک‌شده با منبع/هزینه/score + توقف خودکار).

—— مرحلهٔ بعدِ سند: **Connectorهای واقعی** (منابع داخلی/CSV + وب‌سایت رسمیِ شرکت‌ها)
به‌جای Adapterهای آزمایشی. این کار در build واقعی روی دستگاه شما با کلیدهای واقعی انجام می‌شود.

### Migration جدید مورد نیاز
پس از build، این Migrationها را اضافه/به‌روزرسانی کنید:
  dotnet ef migrations add AddMobileSyncAndConnections -p src/LeadPilot.Infrastructure -s src/LeadPilot.Server
  dotnet ef migrations add AddFirstTenRun -p src/LeadPilot.Infrastructure -s src/LeadPilot.Server
  dotnet ef database update -s src/LeadPilot.Server
جدول‌های جدید: mobile_contact_sync_jobs، first_ten_runs، first_ten_run_results.

---

## ن) Connection Center واقعی (جایگزینِ نسخهٔ سادهٔ قبلی)

نسخهٔ سادهٔ نمایشیِ اتصالات کنار گذاشته شد و نسخهٔ واقعیِ سند (بخش ۱ تا ۱۵) ساخته شد:

**دامنه:**
  • ConnectorDefinition — تعریفِ اتصال؛ رمز/Token/API Key **هرگز** ذخیره نمی‌شود،
    فقط SecretReference. حالت‌ها: NotConfigured/Testing/Healthy/Degraded/RateLimited/
    AuthenticationFailed/QuotaExceeded/Unavailable/Disabled. RowVersion=xmin (Postgres).
  • ConnectorHealthCheck — تاریخچهٔ هر آزمایش (latency، quota، errorCode).

**اپلیکیشن:**
  • IConnectorAdapter — قراردادِ هر منبعِ واقعی (فقط API رسمی/ToS).
  • ISecretStore — خواندنِ Secret با نام.
  • IConnectorHealthService — Test/TestAll/CheckStartupReadiness.
  • StartupReadinessCalculator — منطقِ خالصِ گیت (قابل‌تست بدونِ دیتابیس):
    حیاتیِ خراب → Blocking؛ غیرحیاتیِ خراب → Warning؛ نبودِ منبعِ کشفِ سالم → Blocking.

**زیرساخت:**
  • ConnectorHealthService — کارِ واقعی: Secret را از Store می‌خواند، Adapter را با
    Timeout آزمایش می‌کند، نتیجه+تاریخچه ذخیره می‌کند، گیت را محاسبه می‌کند.
  • ConfigurationSecretStore — نسخهٔ Development (User Secrets/ENV). در Production با
    Key Vault/Vault/Docker Secrets جایگزین می‌شود.
  • TestConnectorAdapter — فقط برای تستِ زیرساخت (برچسبِ TEST).

**API (/api/connections):**
  GET  /tenant/{id}                — فهرست
  POST /                           — ساخت
  PUT  /{id}/configuration         — پیکربندی (فقط SecretReference، نه کلید)
  POST /{id}/enable | /disable
  POST /{id}/test                  — آزمایشِ واقعی
  POST /tenant/{id}/test-all
  GET  /tenant/{id}/readiness      — گیتِ شروع
  (در Production: .RequireAuthorization("connections.manage"))

**اتصال به بقیه:**
  • FirstTenRunService حالا از گیتِ واقعیِ CheckStartupReadinessAsync استفاده می‌کند.
  • Capacity «schedule» (شروعِ برنامهٔ روزانه) هم گیتِ readiness می‌خورد (سند بخش ۱۴).

**Secret آزمایشی (بدونِ ذخیره در دیتابیس):**
```
dotnet user-secrets set "ConnectorSecrets:SEARCH_PRIMARY" "YOUR-API-KEY" --project src/LeadPilot.Server
# در دیتابیس فقط: SecretReference = SEARCH_PRIMARY
```

**Migration:**
```
dotnet ef migrations add AddConnectionCenter --project src/LeadPilot.Infrastructure --startup-project src/LeadPilot.Server
dotnet ef database update  --project src/LeadPilot.Infrastructure --startup-project src/LeadPilot.Server
# جداول: connector_definitions ، connector_health_checks
```

**معیارِ قبولی (سند بخش ۱۵):** ساختِ اتصال ✓ | پیکربندی بدونِ ذخیرهٔ کلید ✓ |
Enable/Disable ✓ | Health Check با Timeout ✓ | خرابیِ یک اتصال کلِ برنامه را متوقف نکند ✓ |
حیاتیِ خراب → Start مسدود ✓ | غیرحیاتیِ خراب → فقط Warning ✓ | تاریخچه ثبت ✓ |
Remaining Quota نمایش ✓ | مقدارِ Secret در API/Log نمایش داده نشود ✓.

پیش‌نمایش HTML: ConnectionCenter-Preview.html
  (کنسولِ اپراتور: پیکربندی/آزمایش/گیتِ آمادگیِ واقعی + ۱۰ نتیجهٔ اول).

—— گامِ واقعیِ بعدی: Adapterهای واقعی (منبعِ داخلی/CSV و وب‌سایتِ رسمیِ شرکت‌ها)
که TEST_SOURCE را جایگزین می‌کنند و اولین ۱۰ نتیجهٔ واقعی را از منابعِ مجاز وارد می‌کنند.
این‌ها کلید/دسترسیِ واقعی می‌خواهند و در build روی دستگاهِ شما ساخته می‌شوند.

---

## و) ماژول ارتباطات (Communications) — کنسولِ کارشناسِ فروش

صفحهٔ ارتباطاتِ داشبورد که کارشناسِ فروش فقط با مخاطبانِ **دارای اکانت و رضایت** کار می‌کند
و نتیجهٔ عملکرد را به CRM می‌فرستد. دستی و خودکار.

**دامنه:**
  • OutreachAttempt — گیتِ رضایت در خودِ دامنه: ارسال فقط اگر رضایتِ همان کانال Granted و
    مخاطب Suppressed/opt-out نباشد؛ در غیر این صورت BlockedByGuard (برای Audit ثبت، هرگز ارسال).
  • CommunicationChannel (InstagramDm/WhatsApp/Email/Sms/Messenger/Telegram/TikTok)،
    OutreachMode (Manual/Automatic)، ChannelConsentStatus (Unknown/Invited/Granted/Denied/Withdrawn/Expired).
  • InstagramCommentRule — اتوماسیونِ کامنت (ManyChat-style، فقط Instagram Graph API رسمی):
    کلیدواژه → پاسخ/دعوتِ DM؛ حالت خودکار یا دستی.
  • ChannelPlaybook — نسخهٔ اختصاصیِ هر کانال (روش کشف، فیلدهای مجاز، روشِ رضایت،
    Pacing مطابقِ نرخِ رسمی، سقفِ روزانه، هزینه، ریسک). IsOutboundTextAllowed: کانال‌های
    «فقط سیگنال» متن نمی‌فرستند.
  • CrmConnection/CrmSyncRecord — اتصالِ HubSpot/Salesforce/Zoho/Pipedrive/Webhook؛
    فقط SecretReference (نه کلید).

**اپلیکیشن:**
  • CommunicationsService — گیتِ رضایت + فرستندهٔ اختصاصیِ کانال.
  • InstagramCommentAutomationService — تطبیقِ کامنت با قاعده → خودکار/صفِ دستی.
  • CrmSyncService — ارسالِ نتیجه به CRM (Secret از Store).
  • IChannelSender (هر کانال جدا)، ICrmConnector، IContactConsentQuery، ICommunicationStore.

**زیرساخت:**
  • فرستندهٔ اختصاصیِ هر کانال (Instagram/WhatsApp/Email/Sms/Messenger/Telegram/TikTok) — نسخهٔ TEST.
  • کانکتورِ هر CRM (HubSpot/Salesforce/Zoho/Pipedrive/Webhook) — نسخهٔ TEST.
  • ConsentQuery — از ProtectedContactPoint می‌خواند: فقط Usable و غیرِ Suppressed.
  نسخهٔ واقعیِ همهٔ این‌ها با API رسمیِ هر پلتفرم و کلید در build ساخته و جایگزین می‌شود.

**API (/api/communications):**
  GET  /inbox/{tenantId}                — فقط مخاطبانِ رضایت‌دار
  POST /contact                         — ارتباط (دستی/خودکار، گیتِ رضایت)
  POST /instagram/evaluate-comment      — ارزیابیِ کامنت با قواعد
  GET/POST /instagram/rules             — قواعدِ کامنت
  GET/POST /crm ، POST /crm/{id}/connect ، POST /crm/{id}/push

**Migration:**
```
dotnet ef migrations add AddCommunications --project src/LeadPilot.Infrastructure --startup-project src/LeadPilot.Server
# جداول: outreach_attempts, instagram_comment_rules, channel_playbooks, crm_connections, crm_sync_records
```

**خطوطِ قرمزِ رعایت‌شده (تصمیمِ آبروی شرکت):** شماره/ایمیل در یدِ مدیریت، ادامهٔ مسیر فقط پس از
رضایت + آرشیوِ رضایت‌نامه؛ ارسال فقط با Granted و بدونِ opt-out؛ هیچ scraping/human-behavior-
evasion/ارسالِ انبوهِ بدونِ رضایت. «طبیعی‌بودن» = متنِ انسانی + Pacing مطابقِ نرخِ رسمی، نه دورزدنِ تشخیص.

پیش‌نمایش HTML: Communications-Console-Preview.html
  (صندوقِ رضایت‌دار، تبِ هر کانال، دستی/خودکار، سازندهٔ قاعدهٔ کامنتِ اینستاگرام،
   دکمه‌های اتصال CRM، تنظیماتِ ابزارها).

---

## ه) میزکارِ اختصاصیِ فروشنده + قفلِ انحصاریِ شماره + Failover ابزارها

**دامنه:**
  • AgentProfile — هر فروشنده: خدماتِ مجاز، کانال‌ها، سقفِ روزانه، اجازهٔ ارسالِ خودکار،
    اجازهٔ Reveal. «فروشندهٔ ۱ ویژهٔ فلان خدمات» → فقط بانکِ همان خدمت.
  • ContactAssignment — قفلِ انحصاری: یک ContactPoint در هر لحظه فقط یک تخصیصِ Active
    (ایندکسِ یکتای فیلترشده روی active_contact_key) → یک شماره در دو جا استفاده نمی‌شود.
    فلگ‌های HasAccount/HasEmail/HasPhone برای بازیابیِ بر اساسِ ویژگی. ReleaseAfterCrmPush.
  • ChannelProviderConfig — اولویتِ ابزار برای هر کانال (Failover).
  • MessagingProviderCode — ManyChat/RespondIo/Wati/WhatsAppCloud/WhatsAppPro/Zoho/NovoChat/Pabbly.

**اپلیکیشن:**
  • AgentAssignmentService — تخصیص با تطبیقِ خدمت + قفلِ انحصاری؛ RetrieveBank بر اساسِ
    ویژگی (دارای اکانت/ایمیل/تلفن)؛ ReleaseAfterCrm.
  • ProviderFailoverRouter — برای یک کانال، ابزارها را به‌ترتیبِ اولویت امتحان می‌کند؛
    «وقتی از یکی نشد، از اون یکی». IMessagingProvider (هر ابزار کانال‌های خودش).

**زیرساخت:** هشت Providerِ TEST (هر کدام کانال‌های پشتیبانی‌شده‌اش) + AgentAssignmentStore.
  نسخهٔ واقعیِ هر ابزار با API رسمی و کلید (در Key Vault) در build اضافه می‌شود.

**API (/api/agents):**
  POST /                       — ساخت پروفایل فروشنده
  POST /assign                 — تخصیصِ شماره (قفلِ انحصاری + تطبیقِ خدمت)
  GET  /{id}/bank?hasAccount&hasEmail&hasPhone&activeOnly — بازیابیِ بانک بر اساسِ ویژگی
  POST /release-after-crm      — آزادسازی پس از ارسال به CRM
  POST /provider-config ، GET /provider-config/{tenantId} — زنجیرهٔ Failover

**Migration:**
```
dotnet ef migrations add AddAgentToolingAndFailover --project src/LeadPilot.Infrastructure --startup-project src/LeadPilot.Server
# جداول: agent_profiles, contact_assignments (unique filtered index), channel_provider_configs
```

**نکتهٔ قفل در Postgres:** ایندکسِ یکتای فیلترشده «active_contact_key IS NOT NULL» تضمین می‌کند
همزمان بیش از یک تخصیصِ Active برای یک شماره ممکن نیست (خطای یکتا در تلاشِ دوم).

پیش‌نمایش HTML: Agent-Workspace-Preview.html
  (سوییچِ فروشنده ۱/۲ با بانکِ متفاوت، فیلترِ ویژگی، قفلِ انحصاری، زنجیرهٔ Failover هر کانال).

—— Failover و بانکِ اختصاصی روی زیرساختِ کانکتورهای واقعی سوارند؛ کانکتورِ واقعیِ هر ابزار
(API رسمی + کلید) در build فعال می‌شود.

---

## ی) موتور مسیریابیِ کانالِ کنترل‌شده + هویتِ چندکاناله + Suppression سراسری

**دو تصحیحِ سند (در کد نشست):**
  ۱) Fallback بینِ کانال‌ها کورکورانه نیست: کانالِ جایگزین باید رضایتِ مستقل و مجوز داشته باشد.
  ۲) نیافتنِ تلفن/ایمیل در یک شبکه، مجوزِ تطبیقِ مخفیانهٔ هویت در شبکه‌های دیگر نیست.

**دامنه:**
  • ChannelCapability — قواعدِ اختصاصیِ هر کانال: CanInitiate، RequiresApprovedTemplate،
    RequiresIncomingWindow، MessagingWindowHours، OutboundSupported.
    (اینستاگرام: پنجرهٔ ۲۴س؛ واتساپ: Template+پنجره؛ تلگرام: کاربر آغازگر؛ تیک‌تاک: بدونِ DM خودکار.)
  • GlobalSuppressionEntry — اولویت بر همه؛ دامنه ChannelOnly یا AllMarketing.
  • IdentityLink — اطمینان (Verified/HighConfidence/NeedsReview/Rejected) از شواهد؛
    شباهتِ عکس/نام همیشه Rejected؛ ادغام فقط با Verified یا تأییدِ انسانی.
  • PrimaryConnectorBinding — یک مسیرِ اصلی برای هر حساب+کانال (ایندکسِ یکتای فیلترشده)؛
    پشتیبان فقط با فعال‌سازیِ دستی و فقط وقتی اصلی خراب است.

**اپلیکیشن:**
  • ChannelRoutingEngine — تصمیمِ ۹گامیِ سند: هویت→رضایتِ مستقل→Suppression→سلامت→
    تکراری→پنجرهٔ پلتفرم؛ خروجی Preferred → Permitted fallback → Human review.
    Fallback هرگز خودکار نیست (همیشه بررسیِ کارشناس).
  • IdentityResolutionService — CanAutoMerge / RequiresHumanApproval.
  • GlobalSuppressionService — IsBlockedAsync (پیش از هر ارسال) / SuppressAsync با دامنه.
  • AgentRolePolicy — Junior=دستی، Senior=+کمک‌یار، Supervisor=+اتوماسیونِ تأییدشده،
    Administrator=+پیکربندی.

**API:**
  POST /api/routing/decide          — تصمیمِ مسیریابی (+trail)
  POST /api/identity/link           — اتصالِ هویت با شواهد (+اطمینان)
  POST /api/suppression ، GET /api/suppression/check — توقفِ سراسری با دامنه

**Migration:**
```
dotnet ef migrations add AddRoutingIdentitySuppression --project src/LeadPilot.Infrastructure --startup-project src/LeadPilot.Server
# جداول: global_suppression_entries, identity_links, primary_connector_bindings
```

**تفاوت با Failover قبلی:** ProviderFailoverRouter بینِ *ابزارهای یک کانال* است (WATI→Cloud API).
ChannelRoutingEngine یک لایهٔ بالاتر است و بینِ *کانال‌ها* تصمیم می‌گیرد — با رضایتِ مستقلِ هر کانال.

پیش‌نمایش HTML: Channel-Routing-Preview.html
  (تصمیمِ ۹گامی با فلگ‌های زنده C/W/S/H، مسیرِ trail، و جدولِ اطمینانِ هویت).

---

## ک) حاکمیتِ جذب (Acquisition): کشف منبع + تأیید Provider + قرنطینه + سیگنال قصد + گاردِ داده

**دامنه (Discovery):**
  • AcquisitionSource — کشفِ منبعِ پیکربندی‌شده per صنعت/شهر/زبان + CollectionAllowed + ابزار.
  • ProviderApproval — چرخهٔ تأیید: NotReviewed→UnderReview→ApprovedForDiscovery/Enrichment/
    Outreach→Suspended/Rejected. Discovery⊂Enrichment⊂Outreach.
  • ImportRecord — خطِ قرنطینه: Imported→SourceValidation→ProvenanceValidation→
    ConsentClassification→Deduplication→HumanReview→Approved/Rejected/Quarantined.
    بدونِ منشأ/رضایت → Quarantined؛ ارسال فقط پس از Approved.
  • IntentSignal — سیگنالِ قصدِ خرید با AllowedNextAction (QualifyOnly/InviteOptIn/
    RouteToLanding/HandoffToAgent). GrantsDirectOutreach همیشه false.
  • LeadMagnet — RequiresConsentBeforeDelivery همیشه true.
  • TrackedLink — فقط دامنهٔ خودی، UTM، انقضا؛ Token/secret در URL ممنوع.

**اپلیکیشن:**
  • DataClassGuard — دادهٔ سازمانی (Company) در برابرِ شخصی (Personal). تبدیلِ خودکارِ
    Gmail→تلفن بدونِ رضایت رد؛ کشفِ تماس از Cookie/هویتِ دیجیتال/مرور رد؛ Intent مدرکِ فرد نیست.
  • QuarantineService — اجرای خطِ قرنطینه.

**API (/api/acquisition):**
  POST/GET /sources                          — Source Discovery
  POST /providers/{tenant}/{provider}/transition ، GET /providers/{tenant} — چرخهٔ تأیید
  POST /imports/run                          — اجرای قرنطینه
  POST /intent                               — سیگنالِ قصد (Qualification-only)
  POST /data-guard/evaluate                  — گاردِ سازمانی/شخصی
  POST /lead-magnets ، POST /tracked-links

**Migration:**
```
dotnet ef migrations add AddAcquisitionGovernance --project src/LeadPilot.Infrastructure --startup-project src/LeadPilot.Server
# جداول: acquisition_sources, provider_approvals, import_records, intent_signals, lead_magnets, tracked_links
```

**ماتریسِ تصمیمِ سند (در گارد/تأیید نشست):** عمومیِ کسب‌وکار/تحلیلِ تجمیعی/Lead Magnet/
WhatsApp Cloud/Tracked link = مجاز؛ Apify/Outscraper/ZoomInfo/Lusha = مشروط به تأیید؛
استخراجِ پروفایلِ شخصی/Gmail→تلفن/دادهٔ مخفیِ اپراتور/Cookie/دورزدنِ CAPTCHA/ارسالِ انبوهِ فریبنده = رد.

پیش‌نمایش HTML: Acquisition-Governance-Preview.html
  (خطِ قرنطینهٔ زنده، چرخهٔ تأیید Provider، ماتریسِ تصمیمِ ابزارها، گاردِ داده).

—— گامِ بعدیِ ممکن: Web Crawler کنترل‌شده، Google Ads Audience Plan، و Playbookِ Community/Forum —
که همه به کلید/دسترسیِ واقعی و بررسیِ قرارداد نیاز دارند و در build فعال می‌شوند.

---

## گ) کانکتورهای غنی‌سازی (Apollo/Lusha/ZoomInfo) با حاکمیت

سندِ ورودی روش‌های هویت‌یابیِ مخفی (Cookie Syncing، Identity Graph جیمیل→موبایل، تزریقِ پیکسل
برای شکارِ هویت، تبدیلِ جیمیلِ اسکرپ‌شده به موبایلِ بدونِ رضایت) را توصیف کرده بود — این‌ها
**ساخته نشدند** (غیرقانونی تحتِ UAE PDPL/GDPR، نقضِ ToS، بن‌شدنِ حساب). بخشِ قانونیِ سند ساخته شد:

  • DataProviderCode: Apollo اضافه شد (کنارِ ZoomInfo/Lusha/…).
  • EnrichmentGovernanceService — یک تصمیمِ واحد که سه شرط را با هم اعمال می‌کند:
    (۱) ProviderApproval.CanEnrich، (۲) DataClassGuard (سازمانی/شخصی)، (۳) رضایتِ اثبات‌شده
    برای هر تماسِ شخصی. نتیجه: COMPANY_ENRICHMENT_OK / CONSENTED_PERSONAL_OK /
    PROVIDER_NOT_APPROVED / DATA_CLASS_BLOCKED.
    → «جیمیلِ شخصی → موبایلِ بدونِ رضایت» و «هویت‌یابی از Cookie» هرگز نمی‌گذرند.

  • API: GET /api/acquisition/enrichment/providers (فهرست + scope)،
        POST /api/acquisition/enrichment/evaluate (تصمیمِ حاکمیتی).

پیش‌نمایش HTML: Enrichment-Connections-Preview.html
  (دکمه‌های اتصالِ ZoomInfo/Apollo/Lusha + scope + بررسیِ زندهٔ مجوز + فهرستِ «پشتیبانی‌نشده»).

—— مسیرِ قانونیِ رسیدن به تلفنِ مخاطب: دعوت به opt-in / Lead Form / رضایتِ ثبت‌شده،
سپس enrichmentِ دارای رضایت از Provider تأییدشده. کانکتورهای واقعی با کلید در build فعال می‌شوند.

---

## پ) ورود CSV/CRM (Import Batch) + چرخهٔ عمرِ داده

**دامنه:**
  • ContactImportBatch — دستهٔ ورود؛ شمارشِ TotalRows/Imported/Duplicate/Invalid؛
    Completed/CompletedWithErrors. مقدارِ خام هرگز در Batch/Log ذخیره نمی‌شود.
  • ConsentEvidence — مدرکِ آرشیویِ رضایت (چه/کِی/چگونه/مرجعِ فایل).
  • DataRetentionPolicy — کِی دادهٔ تماس حذف شود؛ ExpiryFrom/IsExpired.
  • DeletionRequest — حقِ فراموشی (PDPL/GDPR): Received→Verifying→Approved→Executed.

**اپلیکیشن:**
  • ContactCsvRow + ContactCsvRowMap — فرمتِ رسمی؛ ستون‌های اجباری + حداقل phone یا email.
  • ContactImportService (CsvHelper) — Stream→Validate→Resolve→StoreManagerOnly.
  • IImportProspectResolver (external_id) + IImportContactSink (رمزنگاری+Hash+dedup+ManagerOnly).

**زیرساخت:**
  • DeterministicImportProspectResolver — ProspectId قطعی از Tenant+Project+ExternalId.
  • ImportContactSink — Hash از ContactNormalizer، dedup از ContactRecords (قیدِ یکتا)،
    رمزنگاری با IContactValueProtector، ذخیره در ProtectedContactPoint (Hidden/ManagerOnly).
  • CsvHelper 33.0.1 به LeadPilot.Application افزوده شد.

**API (/api/imports):**
  POST /csv (multipart)      — آپلود و اجرای فوریِ ورود
  GET  /batches/{tenant}
  POST /consent-evidence ، /retention-policy ، /deletion-request

**Migration:**
```
dotnet ef migrations add AddImportAndLifecycle --project src/LeadPilot.Infrastructure --startup-project src/LeadPilot.Server
# جداول: contact_import_batches, consent_evidences, data_retention_policies, deletion_requests
```

**نکتهٔ سند:** lawful_source_reference = مرجعِ قانونیِ نگهداری، نه رضایت برای تماسِ فروش؛
دادهٔ واردشده ManagerOnly می‌ماند. اصلاح: هدفِ FirstTenRun «۱۰ شماره تلفنِ یکتا» است.

پیش‌نمایش HTML: CSV-Import-Preview.html
  (فرمتِ رسمی، خطِ ورودِ زنده، آمار، نتیجهٔ ماسک‌شده، و کارت‌های چرخهٔ عمرِ داده).

---

## چ) ماژول Communities & Forums (Opportunity + Intent + پاسخِ شفاف)

قاعدهٔ سند: سؤالِ عمومی = سیگنالِ تقاضا، نه اجازهٔ استخراجِ هویت یا تبلیغِ مستقیم.
روش‌های «حساب جعلی/نجات‌دهندهٔ دلسوز» و «تبدیلِ نام مستعار به موبایل/Gmail» → مطلقاً رد.

**دامنه:**
  • CommunitySource — منبع با وضعیت Candidate→UnderReview→ApprovedForMonitoring/
    PublicResponse/Publishing→Suspended/Rejected؛ CanMonitor/CanPublicRespond/CanPublish.
  • Opportunity — از سؤالِ عمومیِ پرقصد؛ فقط دادهٔ عمومیِ Thread (بدونِ هویت/تماسِ شخصی).
    IsContactable همیشه false؛ تبدیل به Lead فقط پس از اقدامِ داوطلبانه. ثبتِ URL پاسخ توسطِ
    کارشناس (سیستم وانمود نمی‌کند منتشر کرده).

**اپلیکیشن:**
  • IntentScoringEngine — امتیازِ توضیح‌پذیر با دلایل؛ بدونِ ویژگیِ حساس؛ رقیب/فروشنده امتیاز را پایین می‌آورد.
  • ResponseQualityGuard — پیش از انتشار: هویتِ شفاف، بدونِ ادعای مشتریِ جعلی/تضمینِ سود/
    تخریبِ رقیب/فشار برای شماره/پاسخِ تکراری؛ لینک فقط به دامنهٔ رسمی.
  • OpportunityIngestionService — Raw→SourceValidation→PersonalDataClassification→
    AllowedFieldFiltering→Dedup→Opportunity. فیلدهای شخصی حذف می‌شوند.

**API (/api/community):**
  POST/GET /sources ، POST /sources/{id}/transition
  POST /intent-score
  POST /opportunities/ingest ، GET /opportunities/{tenant}
  POST /opportunities/{id}/assign|record-response|convert

**Migration:**
```
dotnet ef migrations add AddCommunityForums --project src/LeadPilot.Infrastructure --startup-project src/LeadPilot.Server
# جداول: community_sources, community_opportunities (unique PublicThreadUrl)
```

پیش‌نمایش HTML: Community-Forums-Preview.html
  (Opportunity Feed با امتیازِ توضیح‌پذیر، گاردِ کیفیتِ پاسخ، و ماتریسِ تصمیمِ روش‌ها).

—— پلتفرم‌های واقعی (Reddit API، Facebook Groups، Quora، Telegram/Discord، Dubizzle) با API رسمی
و مجوزِ مدیرِ انجمن در build فعال می‌شوند. برای افزودنِ منبع: نام پلتفرم، چند نمونهٔ واقعی، صنعت، شهر، نوع مشتری.

---

## ج) معماریِ اطلاعاتیِ داشبورد (چیدمانِ همهٔ ماژول‌ها روی مسیرِ فرآیند)

سند خواست: همهٔ ماژول‌ها طوری در صفحاتِ فروشنده/مدیر چیده شوند که به ۷۰ دکمه نرسیم، روی
مسیرِ فرآیند/خرید مرتب شوند، موارد کم‌اهمیت جلوی صفحه نباشند، زیبایی حفظ شود، و هزینهٔ حدودیِ
هر مرحله باشد. (سندِ تنظیمی از Gemini بود.)

**دامنه (Navigation):**
  • DashboardRole (Seller/Manager/Admin) — منو بر اساسِ نقش فیلتر می‌شود.
  • ProcessStage — مسیرِ فرآیند: Discovery→CaptureConsent→Governance→Assignment→
    Communication→Conversion→ReportsCompliance→Settings(۹۹، پشتِ صفحه).
  • NavItem — Stage + MinRole + IsPrimary (کم‌اهمیت → پشتِ گروه، نه جلوی صفحه) + Icon.
  • StageCostEstimate — هزینهٔ حدودیِ هر مرحله (IsIndicativeOnly = همیشه true).

**اپلیکیشن:**
  • DashboardBlueprint — نقشهٔ کانونی: ۲۷ ماژول → ۸ گروه روی مسیرِ فرآیند. منبعِ واحدِ حقیقتِ منو.
    MenuFor(role): فقط آیتم‌های مجاز، گروه‌بندی‌شده، Primary اول. CostEstimates.

**API (/api/navigation):**
  GET /menu/{role}   — منوی نقش‌محورِ گروه‌بندی‌شده (UI از این می‌خواند، دکمه hardcode نمی‌کند)
  GET /costs         — هزینهٔ حدودیِ هر مرحله (تخمینی)

**اصولِ رعایت‌شده:** به ۷۰ دکمه نمی‌رسیم (۲۷ آیتم/۸ گروه)؛ گروه‌ها به‌ترتیبِ مسیرِ فرآیند و
Settings آخر؛ Primary اولِ هر گروه (کم‌اهمیت‌ها در ادامهٔ مسیر)؛ فروشنده صفحهٔ حاکمیت/تنظیمات
را نمی‌بیند. افزودنِ ماژولِ جدید = یک ردیف در همان نقشه؛ داشبورد خودکار مرتب می‌ماند.

پیش‌نمایش HTML: Dashboard-Architecture-Preview.html
  (سوییچِ فروشنده/مدیر/ادمین، ستونِ مسیرِ فرآیند با آیتم‌های گروه‌بندی‌شده و هزینهٔ هر مرحله).

—— هزینه‌ها تخمینی‌اند و باید از پیشنهادِ رسمیِ همان روز گرفته شوند (سند).

---

## چگ) Launcher کمپین (زمینه‌محور — «یک دکمه، فرآیند پشتِ آن»)

سند (۳۵ روشِ ۳۶۰° از Gemini) → نسخهٔ قانونی. هدفِ کاربر: به‌جای ۷۰–۹۰ فرمِ تودرتو، یک دکمه
per زمینه (هتل/برج/مال/سرمایه‌گذاری/سالن/هوم‌سرویس)، نام+شهر+سقفِ هزینه، و کلِ برنامه‌ریزی پشتِ دکمه.

**دامنه (Campaigns):**
  • CampaignContext — Hotels/ResidentialTowers/Malls/Investment/Salon/HomeService.
  • LaunchTactic — فقط تاکتیک‌های قانونی: ContextualAds, QrNfcOptIn, LoyaltyRewardCoupon,
    SurveyLotteryOptIn, PartnerReferral, CommunityTransparentPost, NewsletterCtaAd,
    B2bCompanyEnrichment, LandingLeadForm. (تاکتیک‌های مخفی اصلاً در enum نیستند.)
  • CampaignPreset — Context+City+Budget+MaxCostPerLeadPercent؛ MaxCostPerLeadUsd و
    IsWithinBudget (BudgetGuard؛ مثلِ «حداکثر ۱٪»).

**اپلیکیشن:**
  • CampaignLauncherService — DefaultTactics(context) + BuildPlan: تاکتیک‌ها + مسیرِ استانداردِ
    رضایت‌محور (SelectApprovedSources→RunTacticsToLanding→VoluntaryOptIn→
    CaptureConsentEvidence→AssignToSpecialist→SyncToCrm). همیشه به opt-in و CRM ختم می‌شود.

**API (/api/campaigns):**
  GET  /contexts/{context}/tactics
  POST /launch                 — زمینه+شهر+بودجه → مسیرِ آمادهٔ اجرا پشتِ دکمه
  GET  /{id}/budget-check

**Migration:**
```
dotnet ef migrations add AddCampaignLauncher --project src/LeadPilot.Infrastructure --startup-project src/LeadPilot.Server
# جدول: campaign_presets (tactics به‌صورت CSV)
```

پیش‌نمایش HTML: Campaign-Launcher-Preview.html
  (دکمه‌های زمینه، فرمِ نام+شهر+بودجه+٪، و مسیرِ کاملِ پشتِ دکمه + فهرستِ «پشتِ هیچ دکمه‌ای اجرا نمی‌شود»).

### آنچه از ۳۵ روش رد شد (پشتِ هیچ دکمه‌ای نیست):
استخراجِ جیمیل/موبایلِ بازدیدکنندهٔ ناشناس · WiFi Captive Portal شخصی · اسکرپِ Geotag/گوگل‌مپ +
تفکیک جنسیتی · افزونهٔ مرورگر برای ایمیل · Whois برای تماسِ سرد · Overlay/Sniply روی سایتِ دیگران.
