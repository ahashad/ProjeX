namespace ProjeX.Domain.Enums
{
    public enum VendorStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        Blacklisted = 4,
        PendingApproval = 5
    }

    public enum VendorCategory
    {
        Supplier = 1,
        Contractor = 2,
        Consultant = 3,
        ServiceProvider = 4,
        Technology = 5,
        Professional = 6,
        Other = 7
    }

    public enum ContractType
    {
        FixedPrice = 1,
        TimeAndMaterial = 2,
        CostPlus = 3,
        Retainer = 4,
        FrameworkAgreement = 5,
        ServiceLevel = 6
    }

    public enum ContractStatus
    {
        Draft = 1,
        Active = 2,
        Expired = 3,
        Terminated = 4,
        Suspended = 5,
        Renewed = 6
    }

    public enum PurchaseOrderStatus
    {
        Draft = 1,
        PendingApproval = 2,
        Approved = 3,
        Sent = 4,
        Acknowledged = 5,
        PartiallyReceived = 6,
        Received = 7,
        Invoiced = 8,
        Closed = 9,
        Cancelled = 10
    }

    public enum GoodsReceiptStatus
    {
        Draft = 1,
        Received = 2,
        PartiallyReceived = 3,
        Rejected = 4,
        Approved = 5
    }

    public enum VendorInvoiceStatus
    {
        Received = 1,
        UnderReview = 2,
        Matched = 3,
        Approved = 4,
        PendingPayment = 5,
        Paid = 6,
        Disputed = 7,
        Rejected = 8
    }
}

