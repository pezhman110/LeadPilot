# سند اجرایی نهایی — LeadPilot / «رادار غربال اتصالات»
### Master Build & Execution Specification
**نسخه:** جمع‌بندی کامل همهٔ مکاتبات و فایل‌های ارسالی · **پروژه:** Persian Horizon / Globex / Elaris (Dubai)
**هدف این سند:** یک مرجع واحد که برنامه‌نویس (میلاد/مفتاح) با آن بتواند سیستم را **build، migrate، متصل به خروجی، و اجرا** کند.

---

## ۰) خلاصهٔ مدیریتی (یک نگاه)

LeadPilot یک موتور **کشف تقاضا → رضایت → صلاحیت‌سنجی → فروش → CRM** برای مشتری فردی (B2C) و سرمایه‌گذار است؛ چندزبانه، رضایت‌محور، و برند-ایمن. هستهٔ آن یک **موتور واحدِ config-محور** است که به ۱۶+ خط کسب‌وکار (سالن، هوم‌سرویس، سرمایه‌گذاری، عروسی، …) گسترش می‌یابد.

| بُعد | مقدار |
|---|---|
| استک قفل‌شده | **.NET 10 + PostgreSQL + Blazor Web App** (Modular Monolith + Worker مستقل، DDD) |
| پروژه‌ها | Domain · Application · Infrastructure · Server · Worker · Tests |
| ماژول‌ها | **۲۶ ماژول دامنه** |
| فایل کد | **۱۷۲ فایل C#** |
| جداول (DbSet) | **۵۵ جدول** |
| مسیرهای API | **۸۲ route** |
| تست | **۲۵ فایل تست** |
| پیش‌نمایش HTML | **۲۳ صفحه** (بدون نیاز به build قابل مشاهده) |
| وضعیت کد | **Near-Production — نیازمند Build & Runtime Validation** (هنوز یک‌بار هم کامپایل نشده) |

> **مهم‌ترین واقعیت:** این کد در محیط بدون .NET SDK نوشته و فقط **ساختاری** اعتبارسنجی شده (توازن، نبود typeِ تکراری، resolve شدن DI). گام بعدی و ضروری = `dotnet build` روی دستگاه شما (بخش ۹).

---

## ۱) معماری و ساختار پروژه

```text
LeadPilot/
├── src/
│   ├── LeadPilot.Domain/          # موجودیت‌ها + قواعد کسب‌وکار (بدون وابستگی خارجی)
│   ├── LeadPilot.Application/     # سرویس‌ها + interfaceها (پورت‌ها)
│   ├── LeadPilot.Infrastructure/  # EF Core + پیاده‌سازی interfaceها + کانکتورها
│   ├── LeadPilot.Server/          # ASP.NET Core Minimal API + Blazor + Endpointها
│   └── LeadPilot.Worker/          # BackgroundServiceها (صف، ظرفیت، پردازش)
└── tests/LeadPilot.Tests/         # xUnit
```

**اصول ثابت:** DDD، پورت/آداپتور (interface در Application، پیاده‌سازی در Infrastructure)، همه‌چیز config-محور (بدون hardcode هر دامنه)، خطوط قرمز در خودِ کد اجرا شده‌اند (بخش ۵).

---

## ۲) زنجیرهٔ اجرایی سیستم (Chain) — قلب کل ماجرا

این زنجیره‌ای است که هر سرنخ از ابتدا تا CRM طی می‌کند. **هر ماژول یک حلقهٔ این زنجیره است:**

```text
۱. کشف (Discovery)
   منبع تأییدشده → سیگنال تقاضا / فرصت عمومی / کمپین زمینه‌محور
        │  (هرگز: استخراج مخفی هویت/تلفن/جیمیل)
        ▼
۲. دریافت و رضایت (Capture & Consent)
   Lead Magnet / لندینگ / QR / فرم → opt-in داوطلبانه → مدرک رضایت آرشیو
        │  (تماس تا اینجا = ManagerOnly، غیرقابل ارسال)
        ▼
۳. حاکمیت (Governance)
   قرنطینه → تأیید Provider → گارد دادهٔ سازمانی/شخصی → enrichment مجاز
        ▼
۴. تخصیص (Assignment)
   dedup → قفل انحصاری شماره → تطبیق زبان/مهارت/امتیاز فروشنده → هدف
        ▼
۵. ارتباط (Communication)
   مسیریابی کانال → رضایت مستقل هر کانال → Failover ابزار → ارسال کنترل‌شده
        ▼
۶. تبدیل (Conversion)
   نتیجهٔ تماس → صلاحیت‌سنجی → همگام‌سازی CRM (HubSpot/Zoho)
        ▼
۷. گزارش و انطباق (Reports & Compliance)
   ROI per منبع/محله/زبان → ممیزی → Suppression → نگهداری و حذف (PDPL/GDPR)
```

**خروجی نهایی (Output):** یک **Opportunity آمادهٔ فروش** در HubSpot با فیلدهای: منبع، رضایت (وضعیت + مدرک)، زبان، کانال ترجیحی، سرویس موردعلاقه، بودجه، محله، فوریت، مالک فروش. فروشنده **فرصت** می‌گیرد، نه lead خام.

**تأثیر (Impact):** کاهش هزینهٔ جذب، لید باکیفیت‌تر، حفاظت از برند ۲۶ساله، و انطباق کامل با TDRA/PDPL.

---

## ۳) فهرست کامل ماژول‌ها (وضعیت هر کدام)

| # | ماژول | Namespace | وضعیت | خروجی/نقش در زنجیره |
|---|---|---|---|---|
| 1 | پروژه‌ها | Projects | ساخته‌شده ✅ | تعریف پروژه/خط کسب‌وکار |
| 2 | جست‌وجو | (Search) | ساخته‌شده ✅ | SearchRun + Target Pursuit |
| 3 | رضایت | Consent | ساخته‌شده ✅ | ProtectedContactPoint (رمزنگاری، Hidden/ManagerOnly) |
| 4 | همکاران/فروشندگان | Partners | ساخته‌شده ✅ | امتیاز/زبان/مهارت + PartnerAgreement |
| 5 | اهداف و زنجیرهٔ درآمد | Targets | ساخته‌شده ✅ | هدف‌گذاری مبتنی بر درآمد |
| 6 | سیگنال‌ها | Signals | ساخته‌شده ✅ | ISignalConnector |
| 7 | محتوا (فاز ۴) | Content | ساخته‌شده ✅ | تولید کپی/اسکریپت (نه فایل نهایی) |
| 8 | منابع | Sources | ساخته‌شده ✅ | رجیستری ۱۴ گروه A–N + ۶۸ منبع + Cascade Planner |
| 9 | حذف تکراری | Dedup | ساخته‌شده ✅ | HMAC hash + قید یکتا + Observation + Reservation |
| 10 | صف پردازش | Processing | ساخته‌شده ✅ | صف پایدار Postgres (Worker) |
| 11 | برنامه‌ریز ظرفیت | Capacity | ساخته‌شده ✅ | ۲۸۰۰–۳۰۰۰ تماس/روز، سهمیه |
| 12 | دسترسی به تماس | Consent/Access | ساخته‌شده ✅ | ممیزی-قبل-از-افشا |
| 13 | همگام‌سازی موبایل | MobileSync | ساخته‌شده ✅ | IDeviceContactStore |
| 14 | اجرای ۱۰ نتیجهٔ اول | Acquisition | ساخته‌شده ✅ | ۱۰ شمارهٔ یکتا، ماسک‌شده، پشت gate سلامت |
| 15 | Connection Center | Connections | ساخته‌شده ✅ | SecretReference-only + Health |
| 16 | ارتباطات (کنسول فروش) | Communications | ساخته‌شده ✅ | IChannelSender + گارد رضایت در دامنه |
| 17 | میزکار فروشنده + Failover | Agents | ساخته‌شده ✅ | قفل انحصاری + ProviderFailoverRouter |
| 18 | مسیریابی/هویت/Suppression | Identity + Suppression | ساخته‌شده ✅ | ۹ گام کنترل‌شده + رضایت مستقل کانال |
| 19 | حاکمیت جذب | Discovery | ساخته‌شده ✅ | منبع + تأیید Provider + قرنطینه + گارد داده |
| 20 | کانکتور enrichment | Discovery | ساخته‌شده ✅ | Apollo/Lusha/ZoomInfo با حاکمیت |
| 21 | ورود CSV/CRM | Imports | ساخته‌شده ✅ | CsvHelper + ManagerOnly + dedup |
| 22 | چرخهٔ عمر داده | Lifecycle | ساخته‌شده ✅ | ConsentEvidence + Retention + Deletion |
| 23 | انجمن‌ها و فروم‌ها | Community | ساخته‌شده ✅ | Opportunity + Intent + پاسخ شفاف |
| 24 | معماری داشبورد | Navigation | ساخته‌شده ✅ | Blueprint نقش‌محور + هزینهٔ هر مرحله |
| 25 | Launcher کمپین | Campaigns | ساخته‌شده ✅ | یک دکمه per زمینه، فرآیند پشت آن |
| 26 | CRM | Crm | ساخته‌شده ✅ | ICrmConnector (HubSpot/Zoho/…) |

---

## ۴) آنچه هنوز برای «اجرای واقعی» لازم است (تحلیل شکاف)

این‌ها **عمداً** نیمه‌کاره‌اند چون به **کلید/قرارداد/دستگاه واقعی** نیاز دارند و در محیط بدون SDK قابل انجام نبودند:

| مورد | وضعیت فعلی | برای اجرا لازم است |
|---|---|---|
| **کامپایل** | هرگز build نشده | `dotnet build` روی دستگاه با .NET 10 SDK |
| **کانکتورهای خروجی** | همه **stub تستی** | کلید واقعی: HubSpot، WhatsApp Cloud API، ManyChat/Respond.io، Meta، Apify |
| **Migration دیتابیس** | تعریف‌شده، اجرا نشده | `dotnet ef database update` روی PostgreSQL |
| **صفحات Blazor** | شِما آماده، رندر نشده | build + اجرا |
| **Channel Library (۶۰ کانال)** | سند خوانده شد، هنوز کد نشده | افزودن پس از سبز شدن build |
| **Official Business Website Connector + Google Ads Plan** | معماری آماده | کلید/قرارداد |
| **تولید فایل نهایی محتوا** | متن/اسکریپت آماده | موتور خارجی (Canva/TTS/Video API) |

**interfaceهایی که باید با پیاده‌سازی واقعی جایگزین شوند:**
`IChannelSender`، `ICrmConnector`، `IMessagingProvider`، `ISignalConnector`، `IDeviceContactStore`، `ISecretStore`، `IConnectorHealth`، `ITestCandidateProvider`.
(همگی الان نسخهٔ TEST دارند تا سیستم بدون کلید هم بالا بیاید.)

---

## ۵) خطوط قرمز (در کد اجرا شده — تغییر ندهید)

این‌ها از اسناد حاکمیتی خودِ شما استخراج و در کد **سخت‌کدنشده** (enum/گارد/تست) شده‌اند:

1. **کشف سیگنالِ عمومی، نه استخراج مخفی هویت.** «داشتن تماس ≠ اجازهٔ تماس فروش».
2. تماسِ واردشده **ManagerOnly**؛ اولین تماس = دعوت به opt-in؛ بخش‌بندی **پس از** رضایت + مدرک آرشیو.
3. **ممنوعِ ساخت (حتی در enum تعریف نشده):** Cookie syncing/fingerprinting برای de-anon، تبدیل Gmail شخصی به تلفن بدون رضایت، تزریق پیکسل به سایت دیگران، اسکرپ پروفایل/کامنت به تلفن، حساب جعلی «نجات‌دهندهٔ دلسوز»، دورزدن CAPTCHA/محدودیت پلتفرم، تفکیک جنسیتی برای هدف‌گیری، Whois برای تماس سرد.
4. **گارد دادهٔ سازمانی/شخصی:** enrichment فقط سطح شرکت یا دادهٔ دارای رضایت.
5. رضایت **مستقل هر کانال**؛ fallback بین‌کاناله بدون رضایت مستقل ممنوع.
6. لینک‌یابیِ هویت فقط با مدرک معتبر (OAuth/خوداظهار/CRM/تأیید کارشناس)، **هرگز** شباهت عکس/نام.
7. Framing بازده/تضمین سود پشت gate حقوقی (UAE SCA/CMA).
8. TDRA/PDPL: opt-out مطلق، ساعات مجاز، بدون Spam بدون رضایت.

---

## ۶) مدل داده (۵۵ جدول — گروه‌بندی)

- **هسته:** projects, prospects, protected_contact_points (ContactPoints), consents, consent_evidences
- **منبع/کشف:** source_registry, source_catalog, acquisition_sources, intent_signals, community_sources, community_opportunities
- **dedup/ظرفیت:** contact_records, contact_observations, enrichment_reservations, daily_capacity_plans, processing_jobs
- **حاکمیت:** provider_approvals, import_records, contact_import_batches, data_retention_policies, deletion_requests
- **فروش/ارتباط:** partners, partner_agreements, targets, outreach_attempts, channel_*, identity_links, global_suppression_entries, crm_connections, crm_sync_records
- **کمپین/ناوبری:** campaign_presets (Navigation = static، بدون جدول)

**کنوانسیون‌ها:** RowVersion → `uint` (xmin) با `.IsRowVersion()`؛ nvarchar(max) → `text`؛ قفل‌ها → `FOR UPDATE SKIP LOCKED` / `pg_try_advisory_lock`؛ رمزنگاری AES-256؛ hash با HMAC-SHA256.

---

## ۷) سطح API (۸۲ route — گروه‌های اصلی)

```text
/api/projects · /api/search · /api/reports · /api/consent · /api/partners · /api/targets
/api/content · /api/processing · /api/capacity · /api/connections
/api/communications · /api/agents · /api/routing
/api/acquisition (sources, providers, imports/run, intent, data-guard, enrichment)
/api/imports (csv, batches, consent-evidence, retention-policy, deletion-request)
/api/community (sources, intent-score, opportunities)
/api/navigation (menu/{role}, costs)
/api/campaigns (contexts/{c}/tactics, launch, budget-check)
```

---

## ۸) داشبورد و خروجی (چیدمان روی مسیر فرآیند — نه ۷۰ دکمه)

منو از **یک Blueprint واحد در بک‌اند** می‌آید (`DashboardBlueprint`)؛ UI هیچ دکمه‌ای hardcode نمی‌کند.
- **۲۷ آیتم در ۸ گروه**، مرتب روی مسیر فرآیند (کشف → … → گزارش → تنظیمات آخر).
- **نقش‌محور:** فروشنده فقط میزکار/صندوق/فرصت/نتیجه؛ مدیر/ادمین بیشتر.
- **Primary اول هر گروه؛ کم‌اهمیت‌ها در ادامهٔ مسیر، نه جلوی صفحه.**
- **هزینهٔ حدودی هر مرحله** (تخمینی — از پیشنهاد رسمیِ روز): کشف $30–300/mo · حاکمیت $50–100/seat · ارتباطات $50–400 · CRM تا $500.
- **Launcher زمینه‌محور:** یک دکمه (هتل/برج/مال/سرمایه‌گذاری/سالن/هوم‌سرویس) + نام+شهر+سقف هزینه → کل فرآیند پشت دکمه، با سقف «حداکثر ۱٪» (BudgetGuard).
- **افزودن ماژول جدید = یک ردیف در Blueprint** → داشبورد خودکار مرتب می‌ماند.

---

## ۹) دستور اجرا (گام‌به‌گام برای رسیدن به «دبل‌کلیک و اجرا»)

### گام ۱ — Build
```bash
cd LeadPilot
dotnet restore
dotnet build          # اولین بار احتمالاً چند خطای جزئی → کپی و ارسال برای رفع
dotnet test           # پس از سبز شدن build
```

### گام ۲ — اتصال به خروجی واقعی (بدون این، دادهٔ واقعی جابه‌جا نمی‌شود)
در `src/LeadPilot.Server/appsettings.json` (سمت شما، کلید به Claude داده نشود):
```json
{
  "ConnectionStrings": { "Postgres": "Host=...;Database=leadpilot;Username=...;Password=..." },
  "Security": { "ContactHashKey": "<یک کلید تصادفی قوی>" },
  "Connectors": {
    "HubSpot":  { "AccessToken": "..." },
    "WhatsAppCloud": { "PhoneNumberId": "...", "AccessToken": "..." },
    "ManyChat": { "ApiKey": "..." },
    "Apify":    { "Token": "..." }
  }
}
```
سپس در Connection Center، هر کانکتور را Test کنید تا **سبز** شود (وضعیت Connected).

### گام ۳ — Migration و اجرا
```bash
dotnet ef migrations add InitialCreate --project src/LeadPilot.Infrastructure --startup-project src/LeadPilot.Server
dotnet ef database update --project src/LeadPilot.Infrastructure --startup-project src/LeadPilot.Server
dotnet run --project src/LeadPilot.Server   # سرویس + Blazor بالا می‌آید
dotnet run --project src/LeadPilot.Worker   # صف/ظرفیت/پردازش
```

### فهرست Migrationهای لازم (append-only)
InitialCreate · AddDurableProcessingQueue · AddDailyCapacityPlanner · AddConnectionCenter · MobileSync · AddFirstTenRun · AddCommunications · AddAgentToolingAndFailover · AddRoutingIdentitySuppression · AddAcquisitionGovernance · AddImportAndLifecycle · AddCommunityForums · AddCampaignLauncher
(Navigation ماژول pure/static است، Migration ندارد.)

### نقطهٔ کنترل (این سه باید سبز شوند تا سیستم «زنده» باشد)
1. `dotnet build` بدون خطا.
2. `dotnet ef database update` جداول را می‌سازد.
3. اولین اجرای واقعی: یک پروژه ساخته و در Postgres ذخیره شود؛ Connection Center یک کانکتور سبز نشان دهد؛ اجرای «۱۰ نتیجهٔ اول» ۱۰ شمارهٔ ماسک‌شده بدهد.

---

## ۱۰) گام‌های بعدی پیشنهادی (پس از سبز شدن build)

1. **Channel Library (۶۰ کانال)** از آخرین سند — با فیلتر ۱۲ سؤالی و طبقه‌بندی هزینه، روی Launcher و Community موجود.
2. جایگزینی stubها با کانکتورهای واقعی، یکی‌یکی، با تست Pilot محدود (بدون ارسال در Pilot).
3. صفحات لندینگ واقعی + فرم‌ساز.
4. Web Crawler دامنه‌های تأییدشده + Google Ads Audience Plan (نیازمند قرارداد/کلید).

---

*این سند مرجع واحد است. هر پیش‌نمایش HTML (۲۳ صفحه) نمای بصری یک ماژول را بدون build نشان می‌دهد. راهنمای فنیِ هر ماژول (دامنه/اپلیکیشن/API/Migration) در `BUILD-GUIDE.md` هست.*
