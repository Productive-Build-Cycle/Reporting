# راهنمای جامع Benchmark در .NET

این سند یک مرجع کامل و عملی برای **یادگیری، پیاده‌سازی و تصمیم‌گیری مهندسی مبتنی بر Benchmark** در پروژه‌های .NET (به‌ویژه ASP.NET Core با Clean Architecture) است. هدف، رسیدن به **تصمیم قابل دفاع** بر اساس عدد، حافظه، پایداری و نگه‌داری است.

---

## 1. Benchmark چیست؟

**Benchmark** یعنی اندازه‌گیری عددی، تکرارپذیر و کنترل‌شده‌ی عملکرد کد برای تصمیم فنی. Benchmark حدس نیست؛ **عدد** است.

**چه زمانی؟**

* Queryهای سنگین، Reporting، Dashboard
* APIهای پرترافیک یا Core Business
* تصمیم‌های معماری (EF vs Dapper، Cache یا نه)

**چه زمانی نه؟**

* CRUD ساده، MVP اولیه، Endpoint کم‌مصرف

---

## 2. Benchmark در برابر Monitoring

| Benchmark         | Monitoring           |
| ----------------- | -------------------- |
| قبل از Production | در Production        |
| داده‌ی کنترل‌شده  | داده‌ی واقعی         |
| تصمیم معماری      | کشف و ریشه‌یابی مشکل |

این دو **مکمل** هستند.

---

## 3. ابزار استاندارد .NET

### BenchmarkDotNet

استاندارد صنعتی برای کنترل:

* Warm-up و JIT
* GC و Allocation
* آمار (Mean/Median/Outliers)
* خروجی قابل استناد

---

## 4. مفاهیم پایه (ضروری)

### 4.1 JIT (Just-In-Time)

کامپایل IL به Native در اولین اجرا انجام می‌شود و زمان‌بر است. بدون Warm-up نتیجه غلط می‌شود.

### 4.2 Warm-up

چند اجرای اولیه برای آماده‌سازی Runtime پیش از اندازه‌گیری واقعی.

### 4.3 Iteration و Invocation

* Iteration: تعداد دفعات اندازه‌گیری
* Invocation: تعداد فراخوانی در هر Iteration (برای متدهای خیلی سریع حیاتی)

### 4.4 GC و Generations

* Gen0: اشیای کوتاه‌عمر
* Gen1: متوسط
* Gen2: بلندعمر (گران)
  Allocation زیاد ⇒ GC بیشتر ⇒ Latency بالاتر.

### 4.5 MemoryDiagnoser

نمایش Allocation و تعداد GCها؛ بدون آن فقط زمان را می‌بینید.

### 4.6 Mean vs Median

Mean به Outlier حساس است؛ Median نماینده‌تر است. هر دو را بررسی کنید.

### 4.7 Baseline

مرجع مقایسه برای نسبت عملکرد. بدون Baseline تصمیم ناقص است.

---

## 5. تنظیمات مهم Benchmark

### 5.1 Runtime

نسخه‌ی .NET (مثلاً .NET 8) را صریح مشخص کنید؛ نتایج بین نسخه‌ها فرق دارد.

### 5.2 Release Mode

Benchmark فقط در Release معتبر است. Debug بی‌اعتبار است.

### 5.3 GC Mode

* Server GC برای APIهای پرترافیک
* Concurrent GC برای Latency کمتر

### 5.4 Environment Noise

Browser، Docker، Battery Mode و CPU Throttling نتایج را خراب می‌کنند. محیط پایدار لازم است.

---

## 6. ساختار درست در پروژه

* اجرا روی **برنچ جدا** یا **پروژه‌ی جدا (.Benchmark)**
* ورود کد بنچ‌مارک به main ممنوع
* فقط **نتیجه‌ی تصمیم** (بهینه‌سازی نهایی) Merge شود

---

## 7. چه چیزهایی را Benchmark می‌کنیم؟

* EF Core vs Dapper
* Projection vs Full Entity
* Cached vs Non-cached
* Sync vs Async (با احتیاط)
* Allocation-heavy vs Allocation-free

---

## 8. Micro vs Macro Benchmark

* **Micro**: یک متد/Query
* **Macro**: کل سناریوی API

تصمیم معماری فقط با Micro کافی نیست.

---

## 9. اشتباهات رایج (اجتناب کنید)

* Benchmark برای همه‌ی APIها
* پیچیده‌سازی زودهنگام
* تغییر هم‌زمان چند متغیر
* Console.WriteLine / Logging
* Stopwatch دستی
* نتیجه‌گیری فقط با Mean

---

## 10. فرآیند حرفه‌ای (Workflow)

```
1) پیاده‌سازی ساده و خوانا
2) مانیتورینگ در تست/استیج
3) مشاهده‌ی نیاز/مشکل
4) Benchmark هدفمند
5) انتخاب راه‌حل
6) مستندسازی تصمیم
```

---

## 11. مستندسازی Benchmark

هر Benchmark باید پاسخ دهد:

* مسئله چه بود؟
* چه چیزی مقایسه شد؟
* نتیجه چه بود؟
* چرا این تصمیم گرفته شد؟

عدد بدون Documentation بی‌ارزش است.

---

## 12. ذهنیت درست

Benchmark برای سریع‌ترین عدد نیست؛ برای **انتخاب ساده‌ترین راه‌حلِ کافی** است.

