namespace ProjeX.Domain.Enums
{
    public enum AccountType
    {
        Prospect = 1,
        Customer = 2,
        Partner = 3,
        Competitor = 4,
        Vendor = 5
    }

    public enum AccountStatus
    {
        Active = 1,
        Inactive = 2,
        Prospect = 3,
        Customer = 4,
        Former = 5
    }

    public enum ContactType
    {
        Primary = 1,
        Secondary = 2,
        Technical = 3,
        Financial = 4,
        Legal = 5,
        Executive = 6
    }

    public enum OpportunityStage
    {
        Qualification = 1,
        NeedsAnalysis = 2,
        ProposalDevelopment = 3,
        Negotiation = 4,
        ClosedWon = 5,
        ClosedLost = 6
    }

    public enum OpportunitySource
    {
        Website = 1,
        Referral = 2,
        ColdCall = 3,
        Email = 4,
        SocialMedia = 5,
        Event = 6,
        Advertisement = 7,
        Partner = 8,
        Other = 9
    }

    public enum TenderType
    {
        OpenTender = 1,
        RestrictedTender = 2,
        NegotiatedProcedure = 3,
        CompetitiveDialogue = 4,
        FrameworkAgreement = 5,
        DirectAward = 6
    }

    public enum TenderStatus
    {
        Published = 1,
        InPreparation = 2,
        Submitted = 3,
        UnderEvaluation = 4,
        Awarded = 5,
        Lost = 6,
        Cancelled = 7,
        Withdrawn = 8
    }

    public enum InteractionType
    {
        Email = 1,
        Phone = 2,
        Meeting = 3,
        VideoCall = 4,
        Event = 5,
        Proposal = 6,
        Contract = 7,
        Other = 8
    }

    public enum ActivityType
    {
        Call = 1,
        Email = 2,
        Meeting = 3,
        Task = 4,
        Note = 5,
        Proposal = 6,
        Presentation = 7,
        Demo = 8
    }
}

