## رسائل النظام الودي (FriendlyMessage)

### مسارات

-   POST `api/FriendlyMessage/send` (Player) — إرسال رسالة للأدمن (Body: SendMessageRequestDto)
-   GET `api/FriendlyMessage/inbox` (Admin) — كل الرسائل
-   GET `api/FriendlyMessage/read-messages` (Admin) — المقروءة
-   GET `api/FriendlyMessage/unread-messages` (Admin) — غير المقروءة
-   POST `api/FriendlyMessage/mark/{messageId}` (Admin) — تعليم كمقروء/غير مقروء (Body: MarkMessageDto)
-   POST `api/FriendlyMessage/delete/{messageId}` (Admin) — حذف/استرجاع (Body: MarkMessageDto)
-   GET `api/FriendlyMessage/my-messages` (Player) — رسائل اللاعب
-   POST `api/FriendlyMessage/reply/{playerId}` (Admin) — رد الأدمن على لاعب (Body: SendMessageRequestDto)
