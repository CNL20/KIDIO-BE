
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
            // [FIX] Xóa dữ liệu cũ rác (như Animals, Colors) hoặc data cũ cần update (như My Home) để Seed lại từ đầu danh sách 25 Topics
            if (await db.Topics.IgnoreQueryFilters().AnyAsync(t => t.Name == "Animals" || t.Name == "My Home"))
            {
                // 1. Xóa "thằng cháu" (Vocabularies) trước
                var oldVocabs = await db.Vocabularies.IgnoreQueryFilters().ToListAsync();
                db.Vocabularies.RemoveRange(oldVocabs);

                // 2. Xóa "thằng con" (Lessons)
                var oldLessons = await db.Lessons.IgnoreQueryFilters().ToListAsync();
                db.Lessons.RemoveRange(oldLessons);

                // 3. Cuối cùng mới xóa "thằng cha" (Topics)
                var oldTopics = await db.Topics.IgnoreQueryFilters().ToListAsync();
                db.Topics.RemoveRange(oldTopics);

                await db.SaveChangesAsync();
            }
            var seedTopics = new[]
            {

                new
                {
                    Name = "ABC Adventure",
                    Description = "Learn letters and their sounds",
                    IconUrl = "/assets/ABC adventure.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "ABC Adventure - Level 1",
                            Description = "First lesson for ABC Adventure",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/trace-letter/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "ABC Adventure - Level 2",
                            Description = "Second lesson for ABC Adventure",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/trace-letter/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "ABC Adventure - Level 3",
                            Description = "Third lesson for ABC Adventure",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/trace-letter/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Number Land",
                    Description = "Learn numbers from 1 to 10",
                    IconUrl = "/assets/Numbers.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Number Land - Level 1",
                            Description = "Learn numbers 1 to 3",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/number-land/village\"}",
                            Vocabularies = new (string, string, string)[] {
                                ("One", "Một", "wʌn")
                            }
                        },
                        new
                        {
                            Title = "Number Land - Level 2",
                            Description = "Learn numbers 4 to 6",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/number-land/teen-town\"}",
                            Vocabularies = new (string, string, string)[] {
                                ("Two", "Hai", "tuː")
                            }
                        },
                        new
                        {
                            Title = "Number Land - Level 3",
                            Description = "Learn numbers 7 to 10",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/number-land/tens-mountain\"}",
                            Vocabularies = new (string, string, string)[] {
                                ("Three", "Ba", "θriː")
                            }
                        }
                    }
                },
                new
                {
                    Name = "Color World",
                    Description = "Learn red, blue, and green",
                    IconUrl = "/assets/Color.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Color World - Level 1",
                            Description = "Learn basic colors",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/color-world\"}",
                            Vocabularies = new (string, string, string)[] {
                                ("Red", "Đỏ", "rɛd")
                            }
                        },
                        new
                        {
                            Title = "Color World - Level 2",
                            Description = "Learn more colors",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/color-world/color-hunt\"}",
                            Vocabularies = new (string, string, string)[] {
                                ("Blue", "Xanh dương", "bluː")
                            }
                        },
                        new
                        {
                            Title = "Color World - Level 3",
                            Description = "Master all colors",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/color-world/mix\"}",
                            Vocabularies = new (string, string, string)[] {
                                ("Green", "Xanh lá", "griːn")
                            }
                        }
                    }
                },
                new
                {
                    Name = "Animal Island",
                    Description = "Learn dog, cat, and bird",
                    IconUrl = "/assets/Animals.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Animal Island - Pets",
                            Description = "Learn about pet animals",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/animal-island/pets\"}",
                            Vocabularies = new (string, string, string)[] {
                                ("Dog", "Chó", "dɒg"),
                                ("Cat", "Mèo", "kæt")
                            }
                        },
                        new
                        {
                            Title = "Animal Island - Farm",
                            Description = "Learn about farm animals",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/animal-island/farm\"}",
                            Vocabularies = new (string, string, string)[] {
                                ("Cow", "Bò", "kaʊ"),
                                ("Pig", "Heo", "pɪg")
                            }
                        },
                        new
                        {
                            Title = "Animal Island - Wild",
                            Description = "Learn about wild animals",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/animal-island/wild\"}",
                            Vocabularies = new (string, string, string)[] {
                                ("Lion", "Sư tử", "ˈlaɪən"),
                                ("Tiger", "Hổ", "ˈtaɪgər")
                            }
                        }
                    }
                },
                new
                {
                    Name = "My Home",
                    Description = "Learn easy objects at home",
                    IconUrl = "/assets/Home.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "My Home - Level 1",
                            Description = "First lesson for My Home",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/my-home-game/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                                ("Bed", "Giường", "bɛd"),
                                ("Door", "Cửa", "dɔːr"),
                                ("Window", "Cửa sổ", "wɪn.doʊ"),
                                ("Chair", "Ghế", "tʃɛər"),
                                ("TV", "Tivi", "tiːˈviː")
                            }
                        },
                        new
                        {
                            Title = "My Home - Level 2",
                            Description = "Second lesson for My Home",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/my-home-game/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "My Home - Level 3",
                            Description = "Third lesson for My Home",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/my-home-game/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Body Parts",
                    Description = "Learn head, hands, and feet",
                    IconUrl = "/assets/Body parts.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Body Parts - Level 1",
                            Description = "First lesson for Body Parts",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/adventure/body-parts/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                                ("Head", "Đầu", "hɛd"),
                                ("Hand", "Tay", "hænd")
                            }
                        },
                        new
                        {
                            Title = "Body Parts - Level 2",
                            Description = "Second lesson for Body Parts",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/adventure/body-parts/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Body Parts - Level 3",
                            Description = "Third lesson for Body Parts",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/adventure/body-parts/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Clothes Corner",
                    Description = "Learn shirt, pants, and shoes",
                    IconUrl = "/assets/Clothes.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Clothes Corner - Level 1",
                            Description = "First lesson for Clothes Corner",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Clothes Corner - Level 2",
                            Description = "Second lesson for Clothes Corner",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Clothes Corner - Level 3",
                            Description = "Third lesson for Clothes Corner",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "House Life",
                    Description = "Learn sofa, lamp, and table",
                    IconUrl = "/assets/House Life.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "House Life - Level 1",
                            Description = "First lesson for House Life",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "House Life - Level 2",
                            Description = "Second lesson for House Life",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "House Life - Level 3",
                            Description = "Third lesson for House Life",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Family House",
                    Description = "Learn mother, father, and family",
                    IconUrl = "/assets/Family.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Family House - Level 1",
                            Description = "First lesson for Family House",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Family House - Level 2",
                            Description = "Second lesson for Family House",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Family House - Level 3",
                            Description = "Third lesson for Family House",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Classroom Objects",
                    Description = "Learn book, pencil, and bag",
                    IconUrl = "/assets/Classroom.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Classroom Objects - Level 1",
                            Description = "First lesson for Classroom Objects",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Classroom Objects - Level 2",
                            Description = "Second lesson for Classroom Objects",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Classroom Objects - Level 3",
                            Description = "Third lesson for Classroom Objects",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "School Town",
                    Description = "Learn school places and subjects",
                    IconUrl = "/assets/School.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "School Town - Level 1",
                            Description = "First lesson for School Town",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "School Town - Level 2",
                            Description = "Second lesson for School Town",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "School Town - Level 3",
                            Description = "Third lesson for School Town",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Sports Arena",
                    Description = "Learn football, swim, and run",
                    IconUrl = "/assets/Sports.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Sports Arena - Level 1",
                            Description = "First lesson for Sports Arena",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Sports Arena - Level 2",
                            Description = "Second lesson for Sports Arena",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Sports Arena - Level 3",
                            Description = "Third lesson for Sports Arena",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Transport City",
                    Description = "Learn car, bus, and bike",
                    IconUrl = "/assets/Transport City.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Transport City - Level 1",
                            Description = "First lesson for Transport City",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Transport City - Level 2",
                            Description = "Second lesson for Transport City",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Transport City - Level 3",
                            Description = "Third lesson for Transport City",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Daily Life",
                    Description = "Learn morning and evening routines",
                    IconUrl = "/assets/Daily Life.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Daily Life - Level 1",
                            Description = "First lesson for Daily Life",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Daily Life - Level 2",
                            Description = "Second lesson for Daily Life",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Daily Life - Level 3",
                            Description = "Third lesson for Daily Life",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Action World",
                    Description = "Learn jump, walk, and play",
                    IconUrl = "/assets/Action world.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Action World - Level 1",
                            Description = "First lesson for Action World",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Action World - Level 2",
                            Description = "Second lesson for Action World",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Action World - Level 3",
                            Description = "Third lesson for Action World",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Story Forest",
                    Description = "Explore your first short story",
                    IconUrl = "/assets/Story forest.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Story Forest - Level 1",
                            Description = "First lesson for Story Forest",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Story Forest - Level 2",
                            Description = "Second lesson for Story Forest",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Story Forest - Level 3",
                            Description = "Third lesson for Story Forest",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Listening Cave",
                    Description = "Listen for words and story clues",
                    IconUrl = "/assets/Listening cave.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Listening Cave - Level 1",
                            Description = "First lesson for Listening Cave",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Listening Cave - Level 2",
                            Description = "Second lesson for Listening Cave",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Listening Cave - Level 3",
                            Description = "Third lesson for Listening Cave",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Reading Castle",
                    Description = "Read short sentences with confidence",
                    IconUrl = "/assets/Reading castle.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Reading Castle - Level 1",
                            Description = "First lesson for Reading Castle",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Reading Castle - Level 2",
                            Description = "Second lesson for Reading Castle",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Reading Castle - Level 3",
                            Description = "Third lesson for Reading Castle",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Character World",
                    Description = "Meet characters and describe them",
                    IconUrl = "/assets/Character world.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Character World - Level 1",
                            Description = "First lesson for Character World",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Character World - Level 2",
                            Description = "Second lesson for Character World",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Character World - Level 3",
                            Description = "Third lesson for Character World",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Adventure Tales",
                    Description = "Understand a complete mini adventure",
                    IconUrl = "/assets/Adventure tales.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Adventure Tales - Level 1",
                            Description = "First lesson for Adventure Tales",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Adventure Tales - Level 2",
                            Description = "Second lesson for Adventure Tales",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Adventure Tales - Level 3",
                            Description = "Third lesson for Adventure Tales",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Feelings Forest",
                    Description = "Talk about happy, sad, and excited",
                    IconUrl = "/assets/Feeling forest.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Feelings Forest - Level 1",
                            Description = "First lesson for Feelings Forest",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Feelings Forest - Level 2",
                            Description = "Second lesson for Feelings Forest",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Feelings Forest - Level 3",
                            Description = "Third lesson for Feelings Forest",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Job Village",
                    Description = "Learn teacher, doctor, and firefighter",
                    IconUrl = "/assets/Job village.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Job Village - Level 1",
                            Description = "First lesson for Job Village",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Job Village - Level 2",
                            Description = "Second lesson for Job Village",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Job Village - Level 3",
                            Description = "Third lesson for Job Village",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Nature Kingdom",
                    Description = "Talk about plants, animals, and weather",
                    IconUrl = "/assets/Nature Kingdom.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Nature Kingdom - Level 1",
                            Description = "First lesson for Nature Kingdom",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Nature Kingdom - Level 2",
                            Description = "Second lesson for Nature Kingdom",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Nature Kingdom - Level 3",
                            Description = "Third lesson for Nature Kingdom",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Space Adventure",
                    Description = "Learn planets, stars, and rockets",
                    IconUrl = "/assets/Space.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Space Adventure - Level 1",
                            Description = "First lesson for Space Adventure",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Space Adventure - Level 2",
                            Description = "Second lesson for Space Adventure",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Space Adventure - Level 3",
                            Description = "Third lesson for Space Adventure",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
                },
                new
                {
                    Name = "Conversation Club",
                    Description = "Practice friendly everyday conversations",
                    IconUrl = "/assets/Conversation.png",
                    Lessons = new[]
                    {
                        new
                        {
                            Title = "Conversation Club - Level 1",
                            Description = "First lesson for Conversation Club",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 1,
                            ContentJson = "{\"route\":\"/video-lesson/level1\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Conversation Club - Level 2",
                            Description = "Second lesson for Conversation Club",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 2,
                            ContentJson = "{\"route\":\"/video-lesson/level2\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        },
                        new
                        {
                            Title = "Conversation Club - Level 3",
                            Description = "Third lesson for Conversation Club",
                            Type = LessonType.Story,
                            Difficulty = DifficultyLevel.Beginner,
                            SkillFocus = SkillType.Vocabulary,
                            DurationSeconds = 120,
                            OrderIndex = 3,
                            ContentJson = "{\"route\":\"/video-lesson/level3\"}",
                            Vocabularies = new (string, string, string)[] {
                            }
                        }
                    }
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
                        IconUrl = seedTopic.IconUrl,
                        OrderIndex = nextTopicOrder++,
                        IsActive = true,
                        Lessons = new List<Lesson>()
                    };

                    await db.Topics.AddAsync(topic);
                    changed = true;
                }
                else if (topic.IconUrl != seedTopic.IconUrl)
                {
                    topic.IconUrl = seedTopic.IconUrl;
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
                    else if (lesson.ContentJson != seedLesson.ContentJson)
                    {
                        lesson.ContentJson = seedLesson.ContentJson;
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
