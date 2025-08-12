## المصادقة (Authentication)

### مسارات

-   POST `api/Auth/login`
-   POST `api/Auth/player-login`
-   POST `api/Auth/register-player`
-   POST `api/Auth/reset-password`
-   POST `api/Auth/logout`

### النماذج

-   LoginDto: `{ email: string, password: string }`
-   PlayerLoginDto: `{ phoneNumber: string, password: string }`
-   RegisterPlayerDto: `{ phoneNumber, password, firstName, lastName }`
-   ResetPasswordDto: `{ phoneNumber, newPassword }`

### الاستجابات

-   نجاح الدخول: `{ success: true, message, token }`
-   فشل: `{ success: false, message }`

### الملاحظات

-   يتطلب إعداد `Jwt:{Issuer,Audience,Key,ExpireDays}`
-   التوكن يحوي Claims للأدوار
