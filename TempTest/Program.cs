using System; var exp = 1779193466L; Console.WriteLine("Exp: " + DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime); Console.WriteLine("Now: " + DateTime.UtcNow);
