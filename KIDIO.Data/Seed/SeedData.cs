using KIDIO.Common.Enums;
using KIDIO.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KIDIO.Data.Seed
{
    public static class SeedData
    {
        public static async Task EnsureSeedDataAsync(KidioDbContext db)
        {
            static string P(string text) => JsonSerializer.Serialize(new { blocks = new[] { new { type = "p", text } } });

            var seedTopics = new[]
            {
                new
                {
                    Name = "Animals",
                    Description = "Animals and sounds",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Farm Animals",
                            Description = "Cow, sheep, pig, horse",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Listening,
                            DurationSeconds = 180,
                            OrderIndex = 1,
                            ContentJson = P("The cow says moo. The sheep says baa. The pig says oink. The horse runs fast."),
                            Vocabularies = new[] { ("Cow", "Bò", "kaʊ"), ("Sheep", "Cừu", "ʃiːp"), ("Pig", "Lợn", "pɪg"), ("Horse", "Ngựa", "hɔːrs") }
                        },
                        new
                        {
                            Title = "Jungle Animals",
                            Description = "Tiger, monkey, elephant, lion",
                            Type = LessonType.Dialogue,
                            Difficulty = DifficultyLevel.Elementary,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 210,
                            OrderIndex = 2,
                            ContentJson = P("Listen to the tiger roar and the monkey chatter. The lion is king."),
                            Vocabularies = new[] { ("Tiger", "Hổ", "ˈtaɪɡər"), ("Monkey", "Khỉ", "ˈmʌŋki"), ("Elephant", "Voi", "ˈɛlɪfənt"), ("Lion", "Sư tử", "ˈlaɪən") }
                        }
                    }
                },
                new
                {
                    Name = "Colors",
                    Description = "Basic colors",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Colors 1",
                            Description = "Primary colors",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Speaking,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = P("Red, blue, green, yellow."),
                            Vocabularies = new[] { ("Red", "Đỏ", "rɛd"), ("Blue", "Xanh dương", "bluː"), ("Green", "Xanh lá", "griːn"), ("Yellow", "Vàng", "ˈjɛloʊ") }
                        }
                    }
                },
                new
                {
                    Name = "Food",
                    Description = "Food and meals",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Fruits",
                            Description = "Common fruits",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 150,
                            OrderIndex = 1,
                            ContentJson = P("Apple, banana, orange, mango."),
                            Vocabularies = new[] { ("Apple", "Táo", "ˈæpəl"), ("Banana", "Chuối", "bəˈnænə"), ("Orange", "Cam", "ˈɒrɪndʒ"), ("Mango", "Xoài", "ˈmæŋɡoʊ") }
                        },
                        new
                        {
                            Title = "Vegetables",
                            Description = "Veggies at the market",
                            Type = LessonType.Dialogue,
                            Difficulty = DifficultyLevel.Elementary,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 160,
                            OrderIndex = 2,
                            ContentJson = P("Carrot, potato, tomato, cucumber."),
                            Vocabularies = new[] { ("Carrot", "Cà rốt", "ˈkærət"), ("Potato", "Khoai tây", "pəˈteɪtoʊ"), ("Tomato", "Cà chua", "təˈmeɪtoʊ"), ("Cucumber", "Dưa chuột", "ˈkjuːkʌmbər") }
                        }
                    }
                },
                new
                {
                    Name = "Transport",
                    Description = "Vehicles and movement",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Transport 1",
                            Description = "Car, bus, bike, motorcycle",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Listening,
                            DurationSeconds = 140,
                            OrderIndex = 1,
                            ContentJson = P("I go to school by bus. My dad drives a car. I ride a bicycle. He rides a motorcycle."),
                            Vocabularies = new[] { ("Car", "Xe hơi", "kɑːr"), ("Bus", "Xe buýt", "bʌs"), ("Bicycle", "Xe đạp", "ˈbaɪsɪkəl"), ("Motorcycle", "Xe máy", "ˈmoʊtərˌsaɪkəl") }
                        }
                    }
                },
                new
                {
                    Name = "Family",
                    Description = "Family members",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Family Members",
                            Description = "Mother, father, brother, sister",
                            Type = LessonType.Dialogue,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Speaking,
                            DurationSeconds = 130,
                            OrderIndex = 1,
                            ContentJson = P("My mother is kind. My brother likes to play. My sister is cute."),
                            Vocabularies = new[] { ("Mother", "Mẹ", "ˈmʌðər"), ("Father", "Cha", "ˈfɑːðər"), ("Brother", "Anh/em trai", "ˈbrʌðər"), ("Sister", "Chị/em gái", "ˈsɪstər") }
                        }
                    }
                },
                new
                {
                    Name = "Numbers",
                    Description = "Counting to ten",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Counting",
                            Description = "1 to 10",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Listening,
                            DurationSeconds = 100,
                            OrderIndex = 1,
                            ContentJson = P("One, two, three, four, five, six, seven, eight, nine, ten."),
                            Vocabularies = new[] { ("One", "Một", "wʌn"), ("Two", "Hai", "tuː"), ("Three", "Ba", "θriː"), ("Four", "Bốn", "fɔːr") }
                        }
                    }
                },
                new
                {
                    Name = "Shapes",
                    Description = "Square, circle, triangle",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Shapes 1",
                            Description = "Basic shapes",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 90,
                            OrderIndex = 1,
                            ContentJson = P("Circle, square, triangle, rectangle."),
                            Vocabularies = new[] { ("Circle", "Hình tròn", "ˈsɜːrkəl"), ("Square", "Hình vuông", "skwɛər"), ("Triangle", "Hình tam giác", "ˈtraɪæŋɡəl"), ("Rectangle", "Hình chữ nhật", "ˈrɛkˌtæŋɡəl") }
                        }
                    }
                },
                new
                {
                    Name = "Weather",
                    Description = "Sunny, rainy, windy",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Weather 1",
                            Description = "Talk about weather",
                            Type = LessonType.Dialogue,
                            Difficulty = DifficultyLevel.Elementary,
                            SkillFocus = SkillType.Listening,
                            DurationSeconds = 110,
                            OrderIndex = 1,
                            ContentJson = P("It is sunny today. It might rain tomorrow. It is cloudy."),
                            Vocabularies = new[] { ("Sunny", "Nắng", "ˈsʌni"), ("Rainy", "Mưa", "ˈreɪni"), ("Windy", "Có gió", "ˈwɪndi"), ("Cloudy", "Nhiều mây", "ˈklaʊdi") }
                        }
                    }
                },
                new
                {
                    Name = "School",
                    Description = "Classroom words",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "At School",
                            Description = "Teacher, desk, book, pencil",
                            Type = LessonType.Dialogue,
                            Difficulty = DifficultyLevel.Elementary,
                            SkillFocus = SkillType.Speaking,
                            DurationSeconds = 140,
                            OrderIndex = 1,
                            ContentJson = P("The teacher opens the book. We sit at our desks. I have a pencil."),
                            Vocabularies = new[] { ("Teacher", "Giáo viên", "ˈtiːtʃər"), ("Book", "Sách", "bʊk"), ("Desk", "Bàn học", "dɛsk"), ("Pencil", "Bút chì", "ˈpɛnsəl") }
                        }
                    }
                },

                new
                {
                    Name = "Alphabet",
                    Description = "Learn and trace letters A to Z",
                    Lessons = Enumerable.Range('A', 26).Select((c, index) => new
                    {
                        Title = $"Letter {(char)c}",
                        Description = $"Trace and learn the letter {(char)c}",
                        Type = LessonType.Alphabet,
                        Difficulty = DifficultyLevel.Beginner,
                        SkillFocus = SkillType.Writing,
                        DurationSeconds = 60,
                        OrderIndex = index + 1,
                        ContentJson = P($"Learn how to write the letter {(char)c}."),
                        Vocabularies = new (string, string, string)[0]
                    }).ToArray()
                }
            };

            var nextTopicOrder = (await db.Topics.IgnoreQueryFilters().MaxAsync(t => (int?)t.OrderIndex) ?? 0) + 1;

            foreach (var seedTopic in seedTopics)
            {
                var topic = await db.Topics
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(t => t.Name == seedTopic.Name);

                if (topic == null)
                {
                    topic = new Topic
                    {
                        Name = seedTopic.Name,
                        Description = seedTopic.Description,
                        OrderIndex = nextTopicOrder++,
                        IsActive = true
                    };

                    await db.Topics.AddAsync(topic);
                    await db.SaveChangesAsync();
                }

                foreach (var seedLesson in seedTopic.Lessons)
                {
                    var lesson = await db.Lessons
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(l => l.TopicId == topic.Id && l.Title == seedLesson.Title);

                    if (lesson == null)
                    {
                        lesson = new Lesson
                        {
                            Title = seedLesson.Title,
                            Description = seedLesson.Description,
                            Type = seedLesson.Type,
                            Difficulty = seedLesson.Difficulty,
                            SkillFocus = seedLesson.SkillFocus,
                            DurationSeconds = seedLesson.DurationSeconds,
                            OrderIndex = seedLesson.OrderIndex,
                            IsPublished = true,
                            ContentJson = seedLesson.ContentJson,
                            TopicId = topic.Id
                        };

                        await db.Lessons.AddAsync(lesson);
                        await db.SaveChangesAsync();
                    }

                    var existingVocabsCount = await db.Vocabularies
                        .IgnoreQueryFilters()
                        .CountAsync(v => v.LessonId == lesson.Id);

                    var currentOrderIndex = existingVocabsCount + 1;

                    foreach (var (word, meaning, phoneticText) in seedLesson.Vocabularies)
                    {
                        var exists = await db.Vocabularies
                            .IgnoreQueryFilters()
                            .AnyAsync(v => v.LessonId == lesson.Id && v.Word == word);

                        if (!exists)
                        {
                            await db.Vocabularies.AddAsync(new Vocabulary
                            {
                                Word = word,
                                Meaning = meaning,
                                OrderIndex = currentOrderIndex++,
                                PhoneticText = phoneticText,
                                LessonId = lesson.Id
                            });
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }

            // Seed AchievementDefinitions
            var seedAchievements = new[]
            {
                new AchievementDefinition { Type = "Lessons", Threshold = 1, Name = "Bài Học Đầu Tiên", Description = "Hoàn thành bài học đầu tiên", BadgeUrl = "first_lesson", OrderIndex = 1, IsActive = true },
                new AchievementDefinition { Type = "Lessons", Threshold = 10, Name = "Hoàn Thành 10 Bài", Description = "Hoàn thành 10 bài học", BadgeUrl = "10_lessons", OrderIndex = 2, IsActive = true },
                new AchievementDefinition { Type = "Stars", Threshold = 100, Name = "Đạt 100 Sao", Description = "Tích lũy được 100 sao học tập", BadgeUrl = "100_stars", OrderIndex = 3, IsActive = true },
                new AchievementDefinition { Type = "Streak", Threshold = 7, Name = "Chăm Chỉ 7 Ngày", Description = "Duy trì streak học tập trong 7 ngày liên tiếp", BadgeUrl = "7_streak", OrderIndex = 4, IsActive = true },
                new AchievementDefinition { Type = "Words", Threshold = 10, Name = "Bậc Thầy Phát Âm", Description = "Phát âm chính xác 10 từ vựng", BadgeUrl = "pron_master", OrderIndex = 5, IsActive = true },
                new AchievementDefinition { Type = "Lessons", Threshold = 5, Name = "Nhà Vô Địch Quiz", Description = "Hoàn thành 5 bài Quiz/học", BadgeUrl = "quiz_champ", OrderIndex = 6, IsActive = true },
                new AchievementDefinition { Type = "Lessons", Threshold = 8, Name = "Nhà Thám Hiểm", Description = "Mở khóa tất cả chủ đề", BadgeUrl = "explorer", OrderIndex = 7, IsActive = true },
                new AchievementDefinition { Type = "Lessons", Threshold = 15, Name = "Dũng Sĩ Diệt Boss", Description = "Vượt qua Boss Battle", BadgeUrl = "boss_slayer", OrderIndex = 8, IsActive = true }
            };

            foreach (var sa in seedAchievements)
            {
                var exists = await db.AchievementDefinitions
                    .IgnoreQueryFilters()
                    .AnyAsync(ad => ad.Name == sa.Name);

                if (!exists)
                {
                    await db.AchievementDefinitions.AddAsync(sa);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
