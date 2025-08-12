## الرسائل (Message)

### مسارات

-   POST `api/Message/send` (Player) — إرسال رسالة للأدمن (Body: SendMessageRequestDto)
-   GET `api/Message/inbox` (Admin) — كل الرسائل
-   GET `api/Message/read-messages` (Admin) — الرسائل المقروءة
-   GET `api/Message/unread-messages` (Admin) — الرسائل غير المقروءة
-   POST `api/Message/mark/{messageId}` (Admin) — تعليم كمقروء/غير مقروء (Body: MarkMessageDto)
-   POST `api/Message/delete/{messageId}` (Admin) — حذف/استرجاع (Body: MarkMessageDto)
-   GET `api/Message/my-messages` (Player) — رسائل اللاعب نفسه
-   POST `api/Message/reply/{playerId}` (Admin) — رد الأدمن على لاعب (Body: SendMessageRequestDto)
