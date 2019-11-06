using Microsoft.AspNetCore.Identity;
using Lamp.Interfaces;
using Lamp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext db)
        {
            db.Database.EnsureCreated();
        }

        public static void Initialize(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IKeyGenerator keyGenerator)
        {
            // Database
            db.Database.EnsureCreated();

            if (db.Users.Any())
            {
                return;
            }

            // Users
            //var user1 = new ApplicationUser
            //{
            //    FirstName = "Natan",
            //    LastName = "Tapia",
            //    UserName = "njtapia@hotmail.com",
            //    Email = "njtapia@hotmail.com"
            //};
            //userManager.CreateAsync(user1, "Test-1234");

            //var user2 = new ApplicationUser
            //{
            //    FirstName = "Emily",
            //    LastName = "Tapia",
            //    UserName = "emifigz@gmail.com",
            //    Email = "emifigz@gmail.com"
            //};
            //userManager.CreateAsync(user2, "Test-1234");

            //var user3 = new ApplicationUser
            //{
            //    FirstName = "Wesley",
            //    LastName = "Figueroa",
            //    UserName = "wesfigz@gmail.com",
            //    Email = "wesfigz@gmail.com"
            //};
            //userManager.CreateAsync(user3, "Test-1234");

            var users = new ApplicationUser[10];
            for (int i = 0; i <= users.GetUpperBound(0); i++)
            {
                users[i] = new ApplicationUser
                {
                    FirstName = "User",
                    LastName = i.ToString("0"),
                    UserName = "test" + i.ToString() + "@test.com",
                    Email = "test" + i.ToString() + "@test.com"
                };
                userManager.CreateAsync(users[i], "Test-1234").Wait();
            }
            db.SaveChanges();

            // Groups
            var groups = new Group[]
            {
                new Group
                {
                    Id = keyGenerator.GenerateKey(),
                    Name = "Metrowest",
                    UsersCanScheduleOthers = true,
                    Members = new Member[]
                    {
                        new Member
                        {
                            User = users[0],
                            Role = GroupRole.Administrator,
                            Approved = true
                        },
                        new Member
                        {
                            User = users[2],
                            Role = GroupRole.Publisher,
                            Approved = true
                        },
                        new Member
                        {
                            User = users[1],
                            Role = GroupRole.Publisher,
                            Approved = true
                        },
                        new Member
                        {
                            User = users[3],
                            Role = GroupRole.Assistant,
                            Approved = true
                        },
                        new Member
                        {
                            User = users[4],
                            Role = GroupRole.Publisher,
                            Approved = false
                        },
                        new Member
                        {
                            User = users[5],
                            Role = GroupRole.Publisher,
                            Approved = false
                        },
                        new Member
                        {
                            User = users[6],
                            Role = GroupRole.Publisher,
                            Approved = false
                        },
                        new Member
                        {
                            User = users[7],
                            Role = GroupRole.Publisher,
                            Approved = false
                        }
                    },
                    Locations = new Location[]
                    {
                        new Location
                        {
                            Name = "Location 0-0"
                        },
                        new Location
                        {
                            Name = "Location 0-3"
                        },
                        new Location
                        {
                            Name = "Location 0-2"
                        },
                        new Location
                        {
                            Name = "Location 0-1"
                        },
                    }
                },
                new Group
                {
                    Id = keyGenerator.GenerateKey(),
                    Name = "MCO Airport",
                    UsersCanScheduleOthers = true,
                    Members = new Member[]
                    {
                        new Member
                        {
                            User = users[0],
                            Role = GroupRole.Publisher,
                            Approved = true
                        },
                        new Member
                        {
                            User = users[1],
                            Role = GroupRole.Administrator,
                            Approved = true
                        }
                    },
                    Locations = new Location[]
                    {
                        new Location
                        {
                            Name = "Location 1-0"
                        },
                        new Location
                        {
                            Name = "Location 1-1"
                        }
                    }
                },
                new Group
                {
                    Id = keyGenerator.GenerateKey(),
                    Name = "Port Canaveral",
                    UsersCanScheduleOthers = true,
                    Members = new Member[]
                    {
                        new Member
                        {
                            User = users[0],
                            Role = GroupRole.Publisher,
                            Approved = false
                        },
                        new Member
                        {
                            User = users[1],
                            Role = GroupRole.Administrator,
                            Approved = true
                        }
                    }
                }
            };

            foreach (Group g in groups)
            {
                db.Groups.Add(g);
            }

            db.SaveChanges();
        }
    }
}