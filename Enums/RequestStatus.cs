namespace RequestLifeCycle.Enums
{
    public enum RequestStatus
    {
        Pending = 1,    // الطلب مفتوح وفي انتظار عروض المحلات
        Accepted = 2,   // العميل قبل عرضاً من أحد المحلات
        InProgress = 3, // المحل بدأ في عملية الصيانة فعلياً
        Completed = 4,  // المحل انتهى من الصيانة بنجاح
        Cancelled = 5   // العميل ألغى الطلب
    }
}