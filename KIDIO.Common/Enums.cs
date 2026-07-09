using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIDIO.Common.Enums
{
    public enum UserRole
    {
        Parent = 1,
        Child = 2,
        Admin = 3
    }

    public enum SkillType
    {
        Listening = 1,
        Speaking = 2,
        Vocabulary = 3,
        Pronunciation = 4,
        Writing = 5
    }

    public enum LessonType
    {
        Story = 1,
        Dialogue = 2,
        VideoShort = 3,
        PronunciationDrill = 4,
        Alphabet = 5
    }

    public enum DifficultyLevel
    {
        Beginner = 1,
        Elementary = 2,
        PreIntermediate = 3
    }

    public enum AccessType
    {
        Free = 0,
        Premium = 1
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Success = 2,
        Failed = 3,
        Cancelled = 4
    }

    public enum PaymentMethod
    {
        PayOS = 1,
        VNPay = 2,
        MoMo = 3,
        Stripe = 4
    }
}
