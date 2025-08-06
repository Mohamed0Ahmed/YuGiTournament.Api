// ========================================
// Friendly Message System TypeScript Types
// ========================================

// DTOs (Using same DTOs as original message system)
export interface SendMessageRequestDto {
    content: string;
}

export interface MarkMessageDto {
    marked: boolean;
}

export interface FriendlyMessage {
    id: number;
    senderId: string;
    senderFullName: string;
    senderPhoneNumber: string;
    content: string;
    isRead: boolean;
    isDeleted: boolean;
    sentAt: string;
    isFromAdmin: boolean;
}

// API Response Types
export interface FriendlyMessageResponse {
    success: boolean;
    message: string;
    messages?: FriendlyMessage[];
}

// API Service Interface
export interface IFriendlyMessageService {
    // Player Actions
    sendMessageToAdmin(content: string): Promise<FriendlyMessageResponse>;
    getMyMessages(): Promise<FriendlyMessageResponse>;

    // Admin Actions
    getInbox(): Promise<FriendlyMessageResponse>;
    getReadMessages(): Promise<FriendlyMessageResponse>;
    getUnreadMessages(): Promise<FriendlyMessageResponse>;
    markAsRead(
        messageId: number,
        marked: boolean
    ): Promise<FriendlyMessageResponse>;
    deleteMessage(
        messageId: number,
        marked: boolean
    ): Promise<FriendlyMessageResponse>;
    sendAdminReply(
        playerId: string,
        content: string
    ): Promise<FriendlyMessageResponse>;
}

// Endpoint Constants (Same pattern as original message system)
export const FRIENDLY_MESSAGE_ENDPOINTS = {
    // Player Actions
    SEND_MESSAGE: "/api/FriendlyMessage/send",
    GET_MY_MESSAGES: "/api/FriendlyMessage/my-messages",

    // Admin Actions
    GET_INBOX: "/api/FriendlyMessage/inbox",
    GET_READ_MESSAGES: "/api/FriendlyMessage/read-messages",
    GET_UNREAD_MESSAGES: "/api/FriendlyMessage/unread-messages",
    MARK_MESSAGE: (messageId: number) =>
        `/api/FriendlyMessage/mark/${messageId}`,
    DELETE_MESSAGE: (messageId: number) =>
        `/api/FriendlyMessage/delete/${messageId}`,
    SEND_ADMIN_REPLY: (playerId: string) =>
        `/api/FriendlyMessage/reply/${playerId}`,
} as const;

// Validation Functions (Same as original message system)
export const validateMessageContent = (content: string): string[] => {
    const errors: string[] = [];

    if (!content || content.trim().length === 0) {
        errors.push("محتوى الرسالة مطلوب");
    }

    if (content && content.trim().length < 5) {
        errors.push("الرسالة يجب أن تكون أكثر من 5 أحرف");
    }

    if (content && content.trim().length > 2000) {
        errors.push("الرسالة يجب أن تكون أقل من 2000 حرف");
    }

    return errors;
};

// Utility Functions (Basic ones for message handling)
export const formatMessageDate = (dateString: string): string => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInMs = now.getTime() - date.getTime();
    const diffInHours = diffInMs / (1000 * 60 * 60);
    const diffInDays = diffInHours / 24;

    if (diffInHours < 1) {
        return "منذ قليل";
    } else if (diffInHours < 24) {
        return `منذ ${Math.floor(diffInHours)} ساعة`;
    } else if (diffInDays < 7) {
        return `منذ ${Math.floor(diffInDays)} يوم`;
    } else {
        return date.toLocaleDateString("ar-EG", {
            year: "numeric",
            month: "long",
            day: "numeric",
        });
    }
};

export const getMessageTypeLabel = (isFromAdmin: boolean): string => {
    return isFromAdmin ? "رد الإدارة" : "رسالة اللاعب";
};

export const getMessageStatusLabel = (
    isRead: boolean,
    isFromAdmin: boolean
): string => {
    if (isFromAdmin) {
        return "تم الإرسال";
    }
    return isRead ? "تم القراءة" : "غير مقروءة";
};

// Message Sorting and Filtering
export const sortMessagesByDate = (
    messages: FriendlyMessage[],
    ascending: boolean = false
): FriendlyMessage[] => {
    return [...messages].sort((a, b) => {
        const dateA = new Date(a.sentAt).getTime();
        const dateB = new Date(b.sentAt).getTime();
        return ascending ? dateA - dateB : dateB - dateA;
    });
};

export const filterMessagesByType = (
    messages: FriendlyMessage[],
    isFromAdmin?: boolean
): FriendlyMessage[] => {
    if (isFromAdmin === undefined) {
        return messages;
    }
    return messages.filter((msg) => msg.isFromAdmin === isFromAdmin);
};

export const filterMessagesByReadStatus = (
    messages: FriendlyMessage[],
    isRead?: boolean
): FriendlyMessage[] => {
    if (isRead === undefined) {
        return messages;
    }
    return messages.filter((msg) => msg.isRead === isRead);
};
