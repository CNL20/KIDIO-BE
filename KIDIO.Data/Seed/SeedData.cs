using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KIDIO.Common.Enums;
using KIDIO.Data.Entities;
using Microsoft.EntityFrameworkCore;

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
                            Description = "Cow, sheep, pig",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Listening,
                            DurationSeconds = 180,
                            OrderIndex = 1,
                            ContentJson = P("The cow says moo. The sheep says baa. The pig says oink."),
                            Vocabularies = new[] { ("Cow", "Bò", "kaʊ"), ("Sheep", "Cừu", "ʃiːp"), ("Pig", "Lợn", "pɪg") }
                        },
                        new
                        {
                            Title = "Jungle Animals",
                            Description = "Tiger, monkey, elephant",
                            Type = LessonType.Dialogue,
                            Difficulty = DifficultyLevel.Elementary,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 210,
                            OrderIndex = 2,
                            ContentJson = P("Listen to the tiger roar and the monkey chatter."),
                            Vocabularies = new[] { ("Tiger", "Hổ", "ˈtaɪɡər"), ("Monkey", "Khỉ", "ˈmʌŋki"), ("Elephant", "Voi", "ˈɛlɪfənt") }
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
                            Vocabularies = new[] { ("Red", "Đỏ", "rɛd"), ("Blue", "Xanh dương", "bluː"), ("Green", "Xanh lá", "griːn") }
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
                            Vocabularies = new[] { ("Apple", "Táo", "ˈæpəl"), ("Banana", "Chuối", "bəˈnænə"), ("Orange", "Cam", "ˈɒrɪndʒ") }
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
                            Vocabularies = new[] { ("Carrot", "Cà rốt", "ˈkærət"), ("Potato", "Khoai tây", "pəˈteɪtoʊ"), ("Tomato", "Cà chua", "təˈmeɪtoʊ") }
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
                            Description = "Car, bus, bike",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Listening,
                            DurationSeconds = 140,
                            OrderIndex = 1,
                            ContentJson = P("I go to school by bus. My dad drives a car."),
                            Vocabularies = new[] { ("Car", "Xe hơi", "kɑːr"), ("Bus", "Xe buýt", "bʌs"), ("Bicycle", "Xe đạp", "ˈbaɪsɪkəl") }
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
                            Description = "Mother, father, sister",
                            Type = LessonType.Dialogue,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Speaking,
                            DurationSeconds = 130,
                            OrderIndex = 1,
                            ContentJson = P("My mother is kind. My brother likes to play."),
                            Vocabularies = new[] { ("Mother", "Mẹ", "ˈmʌðər"), ("Father", "Cha", "ˈfɑːðər"), ("Brother", "Anh/em trai", "ˈbrʌðər") }
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
                            Vocabularies = new[] { ("One", "Một", "wʌn"), ("Two", "Hai", "tuː"), ("Three", "Ba", "θriː") }
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
                            Vocabularies = new[] { ("Circle", "Hình tròn", "ˈsɜːrkəl"), ("Square", "Hình vuông", "skwɛər"), ("Triangle", "Hình tam giác", "ˈtraɪæŋɡəl") }
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
                            ContentJson = P("It is sunny today. It might rain tomorrow."),
                            Vocabularies = new[] { ("Sunny", "Nắng", "ˈsʌni"), ("Rainy", "Mưa", "ˈreɪni"), ("Windy", "Có gió", "ˈwɪndi") }
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
                            Description = "Teacher, desk, book",
                            Type = LessonType.Dialogue,
                            Difficulty = DifficultyLevel.Elementary,
                            SkillFocus = SkillType.Speaking,
                            DurationSeconds = 140,
                            OrderIndex = 1,
                            ContentJson = P("The teacher opens the book. We sit at our desks."),
                            Vocabularies = new[] { ("Teacher", "Giáo viên", "ˈtiːtʃər"), ("Book", "Sách", "bʊk"), ("Desk", "Bàn học", "dɛsk") }
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
            var changed = false;

            foreach (var seedTopic in seedTopics)
            {
                var topic = await db.Topics
                    .IgnoreQueryFilters()
                    .Include(t => t.Lessons)
                    .ThenInclude(l => l.Vocabularies)
                    .FirstOrDefaultAsync(t => t.Name == seedTopic.Name);

                if (topic == null)
                {
                    topic = new Topic
                    {
                        Name = seedTopic.Name,
                        Description = seedTopic.Description,
                        OrderIndex = nextTopicOrder++,
                        IsActive = true,
                        Lessons = new List<Lesson>()
                    };

                    await db.Topics.AddAsync(topic);
                    changed = true;
                }

                foreach (var seedLesson in seedTopic.Lessons)
                {
                    var lesson = topic.Lessons.FirstOrDefault(l => l.Title == seedLesson.Title);
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
                            Topic = topic,
                            Vocabularies = new List<Vocabulary>()
                        };

                        topic.Lessons.Add(lesson);
                        changed = true;
                    }

                    foreach (var (word, meaning, phoneticText) in seedLesson.Vocabularies)
                    {
                        if (lesson.Vocabularies.Any(v => v.Word == word))
                            continue;

                        lesson.Vocabularies.Add(new Vocabulary
                        {
                            Word = word,
                            Meaning = meaning,
                            OrderIndex = lesson.Vocabularies.Count + 1,
                            PhoneticText = phoneticText
                        });
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                await db.SaveChangesAsync();
            }
        }
    }
}
