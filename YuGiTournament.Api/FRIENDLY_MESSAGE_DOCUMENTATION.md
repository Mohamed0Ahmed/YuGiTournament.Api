# 💬 **نظام الرسائل الودية - Friendly Message System**

## 📋 **نظرة عامة**

نظام **الرسائل الودية** يعمل **بنفس طريقة النظام الأصلي** تماماً، لكن يحفظ الرسائل في جدول **FriendlyMessages** منفصل. **اللاعب** يستطيع إرسال رسائل بـ **Player Role** و**الإدارة** ترد عليه، مع استخدام **ApplicationUser** بدلاً من **FriendlyPlayer**.

---

## 🏗️ **هيكل قاعدة البيانات**

### **جدول FriendlyMessages**

```sql
CREATE TABLE FriendlyMessages (
    Id int IDENTITY(1,1) PRIMARY KEY,
    SenderId nvarchar(450) NOT NULL,
    SenderFullName nvarchar(200) NOT NULL,
    SenderPhoneNumber nvarchar(20) NOT NULL,
    Content nvarchar(2000) NOT NULL,
    IsRead bit NOT NULL DEFAULT 0,
    IsDeleted bit NOT NULL DEFAULT 0,
    SentAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    IsFromAdmin bit NOT NULL DEFAULT 0

    -- SenderId references ApplicationUser.Id
);
```

### **الفهارس للأداء**

-   `SenderId` - للبحث السريع برسائل اللاعب
-   `SentAt` - للترتيب الزمني
-   `IsRead` - لفصل المقروء عن غير المقروء
-   `IsDeleted` - للـ Soft Delete
-   `IsFromAdmin` - لفصل رسائل الإدارة عن رسائل اللاعبين

---

## 🎯 **المفاهيم الأساسية**

### **أنواع الرسائل:**

1. **رسالة اللاعب** (`IsFromAdmin = false`) - اللاعب يرسل للإدارة
2. **رد الإدارة** (`IsFromAdmin = true`) - الإدارة ترد على اللاعب

### **حالات القراءة:**

-   **رسائل اللاعبين**: تبدأ `IsRead = false` حتى تقرأها الإدارة
-   **ردود الإدارة**: تبدأ `IsRead = true` تلقائياً

### **الأمان:**

-   **Player Endpoints**: تتطلب `[Authorize(Roles = "Player")]` - اللاعب يرسل رسائل
-   **Admin Endpoints**: تتطلب `[Authorize(Roles = "Admin")]` - الإدارة ترد وتدير الرسائل

---

## 🔗 **API Endpoints**

### **📤 إرسال الرسائل**

#### **إرسال رسالة للإدارة (Player)**

```bash
POST /api/FriendlyMessage/send
Authorization: Bearer {player_token}
Content-Type: application/json

{
    "content": "مرحباً، أريد الاستفسار عن مباراتي الأخيرة"
}
```

**النتيجة:**

```json
{
    "success": true,
    "message": "تم إرسال الرسالة بنجاح"
}
```

#### **عرض رسائلي (Player)**

```bash
GET /api/FriendlyMessage/my-messages
Authorization: Bearer {player_token}
```

#### **إرسال رد من الإدارة (Admin)**

```bash
POST /api/FriendlyMessage/reply/{playerId}
Authorization: Bearer {admin_token}
Content-Type: application/json

{
    "content": "مرحباً، سنتابع استفسارك ونرد عليك قريباً"
}
```

### **📥 استقبال الرسائل (Admin)**

#### **جميع الرسائل**

```bash
GET /api/FriendlyMessage/inbox
Authorization: Bearer {admin_token}
```

#### **الرسائل غير المقروءة**

```bash
GET /api/FriendlyMessage/unread-messages
Authorization: Bearer {admin_token}
```

#### **الرسائل المقروءة**

```bash
GET /api/FriendlyMessage/read-messages
Authorization: Bearer {admin_token}
```

**مثال للنتيجة:**

```json
{
    "success": true,
    "message": "تم جلب المحادثة",
    "messages": [
        {
            "id": 1,
            "playerId": 5,
            "playerFullName": "محمد علي",
            "content": "مرحباً، أريد الاستفسار عن مباراتي الأخيرة",
            "isRead": true,
            "sentAt": "2024-01-15T10:30:00Z",
            "isFromAdmin": false,
            "messageType": "رسالة اللاعب"
        },
        {
            "id": 2,
            "playerId": 5,
            "playerFullName": "الإدارة",
            "content": "مرحباً محمد، سنتابع استفسارك ونرد عليك قريباً",
            "isRead": true,
            "sentAt": "2024-01-15T11:00:00Z",
            "isFromAdmin": true,
            "messageType": "رد الإدارة"
        }
    ]
}
```

### **✅ إدارة الرسائل**

#### **تمييز رسالة كمقروءة (Admin)**

```bash
POST /api/FriendlyMessage/mark/{messageId}
Authorization: Bearer {admin_token}
Content-Type: application/json

{
    "marked": true
}
```

#### **حذف رسالة (Admin)**

```bash
POST /api/FriendlyMessage/delete/{messageId}
Authorization: Bearer {admin_token}
Content-Type: application/json

{
    "marked": true
}
```

---

## 🚀 **سيناريوهات الاستخدام**

### **السيناريو 1: لاعب يرسل استفسار**

1. **اللاعب يرسل رسالة للإدارة:**

```bash
POST /api/FriendlyMessage/send
Authorization: Bearer {player_token}
{
    "content": "أريد معرفة ترتيبي الحالي في الصايمة"
}
```

2. **الإدارة تشوف الرسائل غير المقروءة:**

```bash
GET /api/FriendlyMessage/unread-messages
Authorization: Bearer {admin_token}
```

3. **الإدارة ترد على اللاعب:**

```bash
POST /api/FriendlyMessage/reply/{playerId}
Authorization: Bearer {admin_token}
{
    "content": "ترتيبك الحالي هو المركز الثالث مع 8 انتصارات صايمة"
}
```

### **السيناريو 2: اللاعب يتابع رسائله**

1. **اللاعب يشوف رسائله:**

```bash
GET /api/FriendlyMessage/my-messages
Authorization: Bearer {player_token}
```

2. **الإدارة تمييز الرسالة كمقروءة:**

```bash
POST /api/FriendlyMessage/mark/{messageId}
Authorization: Bearer {admin_token}
{
    "marked": true
}
```

---

## 🔧 **المميزات التقنية**

### **✅ الأمان:**

-   **Player Endpoints** محمية بـ `[Authorize(Roles = "Player")]`
-   **Admin Endpoints** محمية بـ `[Authorize(Roles = "Admin")]`
-   **نفس نظام الأمان** المستخدم في النظام الأصلي
-   Soft Delete للرسائل

### **✅ الأداء:**

-   Indexes محسنة للبحث السريع
-   **UnitOfWork Pattern** للأداء العالي
-   Queries محسنة مع Entity Framework

### **✅ سهولة الاستخدام:**

-   **نفس واجهة النظام الأصلي** تماماً
-   اللاعب يرسل رسائل بـ Player Role
-   الإدارة ترد وتدير الرسائل

### **✅ التوافقية:**

-   منفصل تماماً عن نظام الرسائل الرئيسي
-   **مرتبط بـ ApplicationUser** (نفس النظام الأصلي)
-   **نفس DTOs** المستخدمة في النظام الأصلي
-   TypeScript Types متاحة للـ Frontend

---

## 📱 **Frontend Integration**

### **استيراد Types:**

```typescript
import {
    FriendlyMessage,
    SendMessageRequestDto,
    FRIENDLY_MESSAGE_ENDPOINTS,
    validateMessageContent,
} from "./friendly-message-types";
```

### **مثال لإرسال رسالة (Player):**

```typescript
const sendMessage = async (content: string) => {
    const errors = validateMessageContent(content);
    if (errors.length > 0) {
        throw new Error(errors[0]);
    }

    const response = await fetch(FRIENDLY_MESSAGE_ENDPOINTS.SEND_MESSAGE, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${playerToken}`,
        },
        body: JSON.stringify({ content }),
    });

    return await response.json();
};
```

### **مثال لعرض رسائلي (Player):**

```typescript
const getMyMessages = async () => {
    const response = await fetch(FRIENDLY_MESSAGE_ENDPOINTS.GET_MY_MESSAGES, {
        method: "GET",
        headers: {
            Authorization: `Bearer ${playerToken}`,
        },
    });

    return await response.json();
};
```

---

## 🔗 **الربط مع النظام الرئيسي**

### **مع ApplicationUser:**

-   كل رسالة مرتبطة بـ `ApplicationUser.Id` (SenderId)
-   أسماء اللاعبين تأتي من `ApplicationUser.FName + LName`
-   أرقام الهواتف من `ApplicationUser.PhoneNumber`

### **مع الأدوار:**

-   **Player Role**: يستطيع إرسال رسائل وعرض رسائله
-   **Admin Role**: يستطيع عرض جميع الرسائل والرد عليها وإدارتها
-   **نفس نظام الأدوار** المستخدم في النظام الأصلي

---

## 🎯 **التطوير المستقبلي**

### **إمكانيات للإضافة:**

-   📄 **Pagination** للرسائل الكثيرة
-   🔔 **Push Notifications** للرسائل الجديدة
-   📎 **File Attachments** للصور والملفات
-   🔍 **Search** في محتوى الرسائل
-   📊 **Advanced Analytics** لنشاط الرسائل
-   👥 **Group Messages** للإعلانات العامة

---

**نظام الرسائل الودية يعمل بنفس طريقة النظام الأصلي تماماً لكن في جدول منفصل! 💬✨**

## ✨ **الخلاصة:**

-   ✅ **Player** يستطيع إرسال رسائل ومراجعة رسائله
-   ✅ **Admin** يستطيع عرض وإدارة جميع الرسائل والرد عليها
-   ✅ **نفس Authorization** النظام الأصلي (Player Role / Admin Role)
-   ✅ **نفس DTOs** المستخدمة في النظام الأصلي
-   ✅ **جدول منفصل** FriendlyMessages
-   ✅ **مرتبط بـ ApplicationUser** بدلاً من FriendlyPlayer

---

_تم تطويره بواسطة فريق YuGi Tournament_ 🎮
