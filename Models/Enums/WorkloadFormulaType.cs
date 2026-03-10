namespace DepartmentLoadApp.Models.Enums
{
    public enum WorkloadFormulaType
    {
        None = 0,

        Discipline = 1,

        BachelorThesisGuidance = 2,
        SpecialistThesisGuidance = 3,
        MasterThesisGuidance = 4,
        ThesisReview = 5,
        ThesisPreReview = 6,
        NormControl = 7,

        StateExam = 8,
        GekWork = 9,

        StudyPractice = 10,
        ProductionPractice = 11,
        PrediplomaPractice = 12,
        ScientificPedagogicalPractice = 13,
        ResearchWork = 14,

        Custom = 99
    }
}