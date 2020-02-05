using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Notes2021Blazor.Shared
{
    public partial class NotesDbContext : IdentityDbContext
    {
        public DbSet<UserData> UserData { get; set; }

        public DbSet<TZone> TZone { get; set; }

        public DbSet<NoteFile> NoteFile { get; set; }
        public DbSet<NoteAccess> NoteAccess { get; set; }
        public DbSet<NoteHeader> NoteHeader { get; set; }
        public DbSet<NoteContent> NoteContent { get; set; }
        public DbSet<Tags> Tags { get; set; }

        public DbSet<Audit> Audit { get; set; }
        public DbSet<HomePageMessage> HomePageMessage { get; set; }
        public DbSet<Mark> Mark { get; set; }
        public DbSet<Search> Search { get; set; }
        public DbSet<Sequencer> Sequencer { get; set; }
        public DbSet<Subscription> Subscription { get; set; }

        public DbSet<SQLFile> SQLFile { get; set; }
        public DbSet<SQLFileContent> SQLFileContent { get; set; }

        public void AddJsonFile(string v, bool optional)
        {
            throw new NotImplementedException();
        }

        public DbSet<LinkedFile> LinkedFile { get; set; }
        public DbSet<LinkQueue> LinkQueue { get; set; }
        public DbSet<LinkLog> LinkLog { get; set; }


        public NotesDbContext(DbContextOptions<NotesDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);


            builder.Entity<IdentityRole>().HasData(new IdentityRole { Name = "User", NormalizedName = "USER", Id = Guid.NewGuid().ToString(), ConcurrencyStamp = Guid.NewGuid().ToString() });
            builder.Entity<IdentityRole>().HasData(new IdentityRole { Name = "Admin", NormalizedName = "ADMIN", Id = Guid.NewGuid().ToString(), ConcurrencyStamp = Guid.NewGuid().ToString() });


            builder.Entity<NoteAccess>()
                .HasKey(new string[] { "UserID", "NoteFileId", "ArchiveId" });

            builder.Entity<NoteHeader>()
                .HasOne(p => p.NoteContent)
                .WithOne(i => i.NoteHeader)
                .HasForeignKey<NoteContent>(b => b.NoteHeaderId);

            builder.Entity<NoteHeader>()
                .HasIndex(new string[] { "NoteFileId" });

            builder.Entity<NoteHeader>()
                .HasIndex(new string[] { "NoteFileId", "ArchiveId" });

            builder.Entity<NoteHeader>()
                .HasIndex(new string[] { "LinkGuid" });

            builder.Entity<Tags>()
                .HasKey(new string[] { "Tag", "NoteHeaderId" });

            builder.Entity<Tags>()
                .HasIndex(new string[] { "NoteFileId" });

            builder.Entity<Tags>()
                .HasIndex(new string[] { "NoteFileId", "ArchiveId" });

            builder.Entity<Sequencer>()
                .HasKey(new string[] { "UserId", "NoteFileId" });

            builder.Entity<Search>()
                .HasKey(new string[] { "UserId" });

            builder.Entity<Mark>()
                .HasKey(new string[] { "UserId", "NoteFileId", /*"ArchiveId",*/ "MarkOrdinal" });

            builder.Entity<Mark>()
                .HasIndex(new string[] { "UserId", "NoteFileId" });

            //builder.Entity<Mark>()
            //    .HasIndex(new string[] { "UserId", "NoteFileId", "NoteOrdinal" });

            builder.Entity<SQLFile>()
                .HasOne(p => p.Content)
                .WithOne(i => i.SQLFile)
                .HasForeignKey<SQLFileContent>(b => b.SQLFileId);



            builder.Entity<TZone>().HasData(
                new TZone { Id = 1, Abbreviation = "GMT", Name = "Greenwich Mean Time", Offset = "UTC+00", OffsetHours = 0, OffsetMinutes = 0 },
                new TZone { Id = 2, Abbreviation = "WAKT", Name = "Wake Island Time", Offset = "UTC+12", OffsetHours = 12, OffsetMinutes = 0 },
                new TZone { Id = 3, Abbreviation = "CHAST", Name = "Chatham Standard Time", Offset = "UTC+12:45", OffsetHours = 12, OffsetMinutes = 45 },
                new TZone { Id = 4, Abbreviation = "NZDT", Name = "New Zealand Daylight Time", Offset = "UTC+13", OffsetHours = 13, OffsetMinutes = 0 },
                new TZone { Id = 5, Abbreviation = "PHOT", Name = "Phoenix Island Time", Offset = "UTC+13", OffsetHours = 13, OffsetMinutes = 0 },
                new TZone { Id = 6, Abbreviation = "TKT", Name = "Tokelau Time", Offset = "UTC+13", OffsetHours = 13, OffsetMinutes = 0 },
                new TZone { Id = 7, Abbreviation = "TOT", Name = "Tonga Time", Offset = "UTC+13", OffsetHours = 13, OffsetMinutes = 0 },
                new TZone { Id = 8, Abbreviation = "CHADT", Name = "Chatham Daylight Time", Offset = "UTC+13:45", OffsetHours = 13, OffsetMinutes = 45 },
                new TZone { Id = 9, Abbreviation = "LINT", Name = "Line Islands Time", Offset = "UTC+14", OffsetHours = 14, OffsetMinutes = 0 },
                new TZone { Id = 10, Abbreviation = "AZOST", Name = "Azores Standard Time", Offset = "UTC-01", OffsetHours = -1, OffsetMinutes = 0 },
                new TZone { Id = 11, Abbreviation = "CVT", Name = "Cape Verde Time", Offset = "UTC-01", OffsetHours = -1, OffsetMinutes = 0 },
                new TZone { Id = 12, Abbreviation = "EGT", Name = "Eastern Greenland Time", Offset = "UTC-01", OffsetHours = -1, OffsetMinutes = 0 },
                new TZone { Id = 13, Abbreviation = "BRST", Name = "Brasilia Summer Time", Offset = "UTC-02", OffsetHours = -2, OffsetMinutes = 0 },
                new TZone { Id = 14, Abbreviation = "FNT", Name = "Fernando de Noronha Time", Offset = "UTC-02", OffsetHours = -2, OffsetMinutes = 0 },
                new TZone { Id = 15, Abbreviation = "GST", Name = "South Georgia and the South Sandwich Islands", Offset = "UTC-02", OffsetHours = -2, OffsetMinutes = 0 },
                new TZone { Id = 16, Abbreviation = "PMDT", Name = "Saint Pierre and Miquelon Daylight time", Offset = "UTC-02", OffsetHours = -2, OffsetMinutes = 0 },
                new TZone { Id = 17, Abbreviation = "UYST", Name = "Uruguay Summer Time", Offset = "UTC-02", OffsetHours = -2, OffsetMinutes = 0 },
                new TZone { Id = 18, Abbreviation = "NDT", Name = "Newfoundland Daylight Time", Offset = "UTC-02:30", OffsetHours = -2, OffsetMinutes = -30 },
                new TZone { Id = 19, Abbreviation = "ADT", Name = "Atlantic Daylight Time", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 20, Abbreviation = "AMST", Name = "Amazon Summer Time (Brazil)[1]", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 21, Abbreviation = "ART", Name = "Argentina Time", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 22, Abbreviation = "BRT", Name = "Brasilia Time", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 23, Abbreviation = "TVT", Name = "Tuvalu Time", Offset = "UTC+12", OffsetHours = 12, OffsetMinutes = 0 },
                new TZone { Id = 24, Abbreviation = "PETT", Name = "Kamchatka Time", Offset = "UTC+12", OffsetHours = 12, OffsetMinutes = 0 },
                new TZone { Id = 25, Abbreviation = "NZST", Name = "New Zealand Standard Time", Offset = "UTC+12", OffsetHours = 12, OffsetMinutes = 0 },
                new TZone { Id = 26, Abbreviation = "MHT", Name = "Marshall Islands", Offset = "UTC+12", OffsetHours = 12, OffsetMinutes = 0 },
                new TZone { Id = 27, Abbreviation = "DDUT", Name = "Dumont d'Urville Time", Offset = "UTC+10", OffsetHours = 10, OffsetMinutes = 0 },
                new TZone { Id = 28, Abbreviation = "EST", Name = "Eastern Standard Time (Australia)", Offset = "UTC+10", OffsetHours = 10, OffsetMinutes = 0 },
                new TZone { Id = 29, Abbreviation = "PGT", Name = "Papua New Guinea Time", Offset = "UTC+10", OffsetHours = 10, OffsetMinutes = 0 },
                new TZone { Id = 30, Abbreviation = "VLAT", Name = "Vladivostok Time", Offset = "UTC+10", OffsetHours = 10, OffsetMinutes = 0 },
                new TZone { Id = 31, Abbreviation = "ACDT", Name = "Australian Central Daylight Savings Time", Offset = "UTC+10:30", OffsetHours = 10, OffsetMinutes = 30 },
                new TZone { Id = 32, Abbreviation = "CST", Name = "Central Summer Time (Australia)", Offset = "UTC+10:30", OffsetHours = 10, OffsetMinutes = 30 },
                new TZone { Id = 33, Abbreviation = "LHST", Name = "Lord Howe Standard Time", Offset = "UTC+10:30", OffsetHours = 10, OffsetMinutes = 30 },
                new TZone { Id = 34, Abbreviation = "AEDT", Name = "Australian Eastern Daylight Savings Time", Offset = "UTC+11", OffsetHours = 11, OffsetMinutes = 0 },
                new TZone { Id = 35, Abbreviation = "BST", Name = "Bougainville Standard Time[4]", Offset = "UTC+11", OffsetHours = 11, OffsetMinutes = 0 },
                new TZone { Id = 36, Abbreviation = "KOST", Name = "Kosrae Time", Offset = "UTC+11", OffsetHours = 11, OffsetMinutes = 0 },
                new TZone { Id = 37, Abbreviation = "CLST", Name = "Chile Summer Time", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 38, Abbreviation = "LHST", Name = "Lord Howe Summer Time", Offset = "UTC+11", OffsetHours = 11, OffsetMinutes = 0 },
                new TZone { Id = 39, Abbreviation = "NCT", Name = "New Caledonia Time", Offset = "UTC+11", OffsetHours = 11, OffsetMinutes = 0 },
                new TZone { Id = 40, Abbreviation = "PONT", Name = "Pohnpei Standard Time", Offset = "UTC+11", OffsetHours = 11, OffsetMinutes = 0 },
                new TZone { Id = 41, Abbreviation = "SAKT", Name = "Sakhalin Island time", Offset = "UTC+11", OffsetHours = 11, OffsetMinutes = 0 },
                new TZone { Id = 42, Abbreviation = "SBT", Name = "Solomon Islands Time", Offset = "UTC+11", OffsetHours = 11, OffsetMinutes = 0 },
                new TZone { Id = 43, Abbreviation = "SRET", Name = "Srednekolymsk Time", Offset = "UTC+11", OffsetHours = 11, OffsetMinutes = 0 },
                new TZone { Id = 44, Abbreviation = "VUT", Name = "Vanuatu Time", Offset = "UTC+11", OffsetHours = 11, OffsetMinutes = 0 },
                new TZone { Id = 45, Abbreviation = "NFT", Name = "Norfolk Time", Offset = "UTC+11:00", OffsetHours = 11, OffsetMinutes = 0 },
                new TZone { Id = 46, Abbreviation = "FJT", Name = "Fiji Time", Offset = "UTC+12", OffsetHours = 12, OffsetMinutes = 0 },
                new TZone { Id = 47, Abbreviation = "GILT", Name = "Gilbert Island Time", Offset = "UTC+12", OffsetHours = 12, OffsetMinutes = 0 },
                new TZone { Id = 48, Abbreviation = "MAGT", Name = "Magadan Time", Offset = "UTC+12", OffsetHours = 12, OffsetMinutes = 0 },
                new TZone { Id = 49, Abbreviation = "MIST", Name = "Macquarie Island Station Time", Offset = "UTC+11", OffsetHours = 11, OffsetMinutes = 0 },
                new TZone { Id = 50, Abbreviation = "CHUT", Name = "Chuuk Time", Offset = "UTC+10", OffsetHours = 10, OffsetMinutes = 0 },
                new TZone { Id = 51, Abbreviation = "FKST", Name = "Falkland Islands Standard Time", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 52, Abbreviation = "GFT", Name = "French Guiana Time", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 53, Abbreviation = "PET", Name = "Peru Time", Offset = "UTC-05", OffsetHours = -5, OffsetMinutes = 0 },
                new TZone { Id = 54, Abbreviation = "CST", Name = "Central Standard Time (North America)", Offset = "UTC-06", OffsetHours = -6, OffsetMinutes = 0 },
                new TZone { Id = 55, Abbreviation = "EAST", Name = "Easter Island Standard Time", Offset = "UTC-06", OffsetHours = -6, OffsetMinutes = 0 },
                new TZone { Id = 56, Abbreviation = "GALT", Name = "Galapagos Time", Offset = "UTC-06", OffsetHours = -6, OffsetMinutes = 0 },
                new TZone { Id = 57, Abbreviation = "MDT", Name = "Mountain Daylight Time (North America)", Offset = "UTC-06", OffsetHours = -6, OffsetMinutes = 0 },
                new TZone { Id = 58, Abbreviation = "MST", Name = "Mountain Standard Time (North America)", Offset = "UTC-07", OffsetHours = -7, OffsetMinutes = 0 },
                new TZone { Id = 59, Abbreviation = "PDT", Name = "Pacific Daylight Time (North America)", Offset = "UTC-07", OffsetHours = -7, OffsetMinutes = 0 },
                new TZone { Id = 60, Abbreviation = "AKDT", Name = "Alaska Daylight Time", Offset = "UTC-08", OffsetHours = -8, OffsetMinutes = 0 },
                new TZone { Id = 61, Abbreviation = "CIST", Name = "Clipperton Island Standard Time", Offset = "UTC-08", OffsetHours = -8, OffsetMinutes = 0 },
                new TZone { Id = 62, Abbreviation = "PST", Name = "Pacific Standard Time (North America)", Offset = "UTC-08", OffsetHours = -8, OffsetMinutes = 0 },
                new TZone { Id = 63, Abbreviation = "AKST", Name = "Alaska Standard Time", Offset = "UTC-09", OffsetHours = -9, OffsetMinutes = 0 },
                new TZone { Id = 64, Abbreviation = "GAMT", Name = "Gambier Islands", Offset = "UTC-09", OffsetHours = -9, OffsetMinutes = 0 },
                new TZone { Id = 65, Abbreviation = "GIT", Name = "Gambier Island Time", Offset = "UTC-09", OffsetHours = -9, OffsetMinutes = 0 },
                new TZone { Id = 66, Abbreviation = "HADT", Name = "Hawaii-Aleutian Daylight Time", Offset = "UTC-09", OffsetHours = -9, OffsetMinutes = 0 },
                new TZone { Id = 67, Abbreviation = "MART", Name = "Marquesas Islands Time", Offset = "UTC-09:30", OffsetHours = -9, OffsetMinutes = -30 },
                new TZone { Id = 68, Abbreviation = "MIT", Name = "Marquesas Islands Time", Offset = "UTC-09:30", OffsetHours = -9, OffsetMinutes = -30 },
                new TZone { Id = 69, Abbreviation = "CKT", Name = "Cook Island Time", Offset = "UTC-10", OffsetHours = -10, OffsetMinutes = 0 },
                new TZone { Id = 70, Abbreviation = "HAST", Name = "Hawaii-Aleutian Standard Time", Offset = "UTC-10", OffsetHours = -10, OffsetMinutes = 0 },
                new TZone { Id = 71, Abbreviation = "HST", Name = "Hawaii Standard Time", Offset = "UTC-10", OffsetHours = -10, OffsetMinutes = 0 },
                new TZone { Id = 72, Abbreviation = "TAHT", Name = "Tahiti Time", Offset = "UTC-10", OffsetHours = -10, OffsetMinutes = 0 },
                new TZone { Id = 73, Abbreviation = "NUT", Name = "Niue Time", Offset = "UTC-11", OffsetHours = -11, OffsetMinutes = 0 },
                new TZone { Id = 74, Abbreviation = "EST", Name = "Eastern Standard Time (North America)", Offset = "UTC-05", OffsetHours = -5, OffsetMinutes = 0 },
                new TZone { Id = 75, Abbreviation = "ECT", Name = "Ecuador Time", Offset = "UTC-05", OffsetHours = -5, OffsetMinutes = 0 },
                new TZone { Id = 76, Abbreviation = "EASST", Name = "Easter Island Standard Summer Time", Offset = "UTC-05", OffsetHours = -5, OffsetMinutes = 0 },
                new TZone { Id = 77, Abbreviation = "CST", Name = "Cuba Standard Time", Offset = "UTC-05", OffsetHours = -5, OffsetMinutes = 0 },
                new TZone { Id = 78, Abbreviation = "PMST", Name = "Saint Pierre and Miquelon Standard Time", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 79, Abbreviation = "PYST", Name = "Paraguay Summer Time (South America)[8]", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 80, Abbreviation = "ROTT", Name = "Rothera Research Station Time", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 81, Abbreviation = "SRT", Name = "Suriname Time", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 82, Abbreviation = "UYT", Name = "Uruguay Standard Time", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 83, Abbreviation = "NST", Name = "Newfoundland Standard Time", Offset = "UTC-03:30", OffsetHours = -3, OffsetMinutes = -30 },
                new TZone { Id = 84, Abbreviation = "NT", Name = "Newfoundland Time", Offset = "UTC-03:30", OffsetHours = -3, OffsetMinutes = -30 },
                new TZone { Id = 85, Abbreviation = "AMT", Name = "Amazon Time (Brazil)[2]", Offset = "UTC-04", OffsetHours = -4, OffsetMinutes = 0 },
                new TZone { Id = 86, Abbreviation = "AST", Name = "Atlantic Standard Time", Offset = "UTC-04", OffsetHours = -4, OffsetMinutes = 0 },
                new TZone { Id = 87, Abbreviation = "BOT", Name = "Bolivia Time", Offset = "UTC-04", OffsetHours = -4, OffsetMinutes = 0 },
                new TZone { Id = 88, Abbreviation = "FKST", Name = "Falkland Islands Summer Time", Offset = "UTC-03", OffsetHours = -3, OffsetMinutes = 0 },
                new TZone { Id = 89, Abbreviation = "CDT", Name = "Cuba Daylight Time[5]", Offset = "UTC-04", OffsetHours = -4, OffsetMinutes = 0 },
                new TZone { Id = 90, Abbreviation = "COST", Name = "Colombia Summer Time", Offset = "UTC-04", OffsetHours = -4, OffsetMinutes = 0 },
                new TZone { Id = 91, Abbreviation = "ECT", Name = "Eastern Caribbean Time (does not recognise DST)", Offset = "UTC-04", OffsetHours = -4, OffsetMinutes = 0 },
                new TZone { Id = 92, Abbreviation = "EDT", Name = "Eastern Daylight Time (North America)", Offset = "UTC-04", OffsetHours = -4, OffsetMinutes = 0 },
                new TZone { Id = 93, Abbreviation = "FKT", Name = "Falkland Islands Time", Offset = "UTC-04", OffsetHours = -4, OffsetMinutes = 0 },
                new TZone { Id = 94, Abbreviation = "GYT", Name = "Guyana Time", Offset = "UTC-04", OffsetHours = -4, OffsetMinutes = 0 },
                new TZone { Id = 95, Abbreviation = "PYT", Name = "Paraguay Time (South America)[9]", Offset = "UTC-04", OffsetHours = -4, OffsetMinutes = 0 },
                new TZone { Id = 96, Abbreviation = "VET", Name = "Venezuelan Standard Time", Offset = "UTC-04:30", OffsetHours = -4, OffsetMinutes = -30 },
                new TZone { Id = 97, Abbreviation = "ACT", Name = "Acre Time", Offset = "UTC-05", OffsetHours = -5, OffsetMinutes = 0 },
                new TZone { Id = 98, Abbreviation = "CDT", Name = "Central Daylight Time (North America)", Offset = "UTC-05", OffsetHours = -5, OffsetMinutes = 0 },
                new TZone { Id = 99, Abbreviation = "COT", Name = "Colombia Time", Offset = "UTC-05", OffsetHours = -5, OffsetMinutes = 0 },
                new TZone { Id = 100, Abbreviation = "CLT", Name = "Chile Standard Time", Offset = "UTC-04", OffsetHours = -4, OffsetMinutes = 0 },
                new TZone { Id = 101, Abbreviation = "ChST", Name = "Chamorro Standard Time", Offset = "UTC+10", OffsetHours = 10, OffsetMinutes = 0 },
                new TZone { Id = 102, Abbreviation = "AEST", Name = "Australian Eastern Standard Time", Offset = "UTC+10", OffsetHours = 10, OffsetMinutes = 0 },
                new TZone { Id = 103, Abbreviation = "CST", Name = "Central Standard Time (Australia)", Offset = "UTC+09:30", OffsetHours = 9, OffsetMinutes = 30 },
                new TZone { Id = 104, Abbreviation = "EEDT", Name = "Eastern European Daylight Time", Offset = "UTC+03", OffsetHours = 3, OffsetMinutes = 0 },
                new TZone { Id = 105, Abbreviation = "EEST", Name = "Eastern European Summer Time", Offset = "UTC+03", OffsetHours = 3, OffsetMinutes = 0 },
                new TZone { Id = 106, Abbreviation = "FET", Name = "Further-eastern European Time", Offset = "UTC+03", OffsetHours = 3, OffsetMinutes = 0 },
                new TZone { Id = 107, Abbreviation = "IDT", Name = "Israel Daylight Time", Offset = "UTC+03", OffsetHours = 3, OffsetMinutes = 0 },
                new TZone { Id = 108, Abbreviation = "IOT", Name = "Indian Ocean Time", Offset = "UTC+03", OffsetHours = 3, OffsetMinutes = 0 },
                new TZone { Id = 109, Abbreviation = "MSK", Name = "Moscow Time", Offset = "UTC+03", OffsetHours = 3, OffsetMinutes = 0 },
                new TZone { Id = 110, Abbreviation = "SYOT", Name = "Showa Station Time", Offset = "UTC+03", OffsetHours = 3, OffsetMinutes = 0 },
                new TZone { Id = 111, Abbreviation = "IRST", Name = "Iran Standard Time", Offset = "UTC+03:30", OffsetHours = 3, OffsetMinutes = 30 },
                new TZone { Id = 112, Abbreviation = "AMT", Name = "Armenia Time", Offset = "UTC+04", OffsetHours = 4, OffsetMinutes = 0 },
                new TZone { Id = 113, Abbreviation = "AZT", Name = "Azerbaijan Time", Offset = "UTC+04", OffsetHours = 4, OffsetMinutes = 0 },
                new TZone { Id = 114, Abbreviation = "GET", Name = "Georgia Standard Time", Offset = "UTC+04", OffsetHours = 4, OffsetMinutes = 0 },
                new TZone { Id = 115, Abbreviation = "GST", Name = "Gulf Standard Time", Offset = "UTC+04", OffsetHours = 4, OffsetMinutes = 0 },
                new TZone { Id = 116, Abbreviation = "MUT", Name = "Mauritius Time", Offset = "UTC+04", OffsetHours = 4, OffsetMinutes = 0 },
                new TZone { Id = 117, Abbreviation = "RET", Name = "R?union Time", Offset = "UTC+04", OffsetHours = 4, OffsetMinutes = 0 },
                new TZone { Id = 118, Abbreviation = "SAMT", Name = "Samara Time", Offset = "UTC+04", OffsetHours = 4, OffsetMinutes = 0 },
                new TZone { Id = 119, Abbreviation = "SCT", Name = "Seychelles Time", Offset = "UTC+04", OffsetHours = 4, OffsetMinutes = 0 },
                new TZone { Id = 120, Abbreviation = "VOLT", Name = "Volgograd Time", Offset = "UTC+04", OffsetHours = 4, OffsetMinutes = 0 },
                new TZone { Id = 121, Abbreviation = "AFT", Name = "Afghanistan Time", Offset = "UTC+04:30", OffsetHours = 4, OffsetMinutes = 30 },
                new TZone { Id = 122, Abbreviation = "IRDT", Name = "Iran Daylight Time", Offset = "UTC+04:30", OffsetHours = 4, OffsetMinutes = 30 },
                new TZone { Id = 123, Abbreviation = "HMT", Name = "Heard and McDonald Islands Time", Offset = "UTC+05", OffsetHours = 5, OffsetMinutes = 0 },
                new TZone { Id = 124, Abbreviation = "MAWT", Name = "Mawson Station Time", Offset = "UTC+05", OffsetHours = 5, OffsetMinutes = 0 },
                new TZone { Id = 125, Abbreviation = "EAT", Name = "East Africa Time", Offset = "UTC+03", OffsetHours = 3, OffsetMinutes = 0 },
                new TZone { Id = 126, Abbreviation = "AST", Name = "Arabia Standard Time", Offset = "UTC+03", OffsetHours = 3, OffsetMinutes = 0 },
                new TZone { Id = 127, Abbreviation = "WAST", Name = "West Africa Summer Time", Offset = "UTC+02", OffsetHours = 2, OffsetMinutes = 0 },
                new TZone { Id = 128, Abbreviation = "USZ1", Name = "Kaliningrad Time", Offset = "UTC+02", OffsetHours = 2, OffsetMinutes = 0 },
                new TZone { Id = 129, Abbreviation = "IBST", Name = "International Business Standard Time", Offset = "UTC+00", OffsetHours = 0, OffsetMinutes = 0 },
                new TZone { Id = 130, Abbreviation = "UCT", Name = "Coordinated Universal Time", Offset = "UTC+00", OffsetHours = 0, OffsetMinutes = 0 },
                new TZone { Id = 131, Abbreviation = "UTC", Name = "Coordinated Universal Time", Offset = "UTC+00", OffsetHours = 0, OffsetMinutes = 0 },
                new TZone { Id = 132, Abbreviation = "WET", Name = "Western European Time", Offset = "UTC+00", OffsetHours = 0, OffsetMinutes = 0 },
                new TZone { Id = 133, Abbreviation = "Z", Name = "Zulu Time (Coordinated Universal Time)", Offset = "UTC+00", OffsetHours = 0, OffsetMinutes = 0 },
                new TZone { Id = 134, Abbreviation = "EGST", Name = "Eastern Greenland Summer Time", Offset = "UTC+00", OffsetHours = 0, OffsetMinutes = 0 },
                new TZone { Id = 135, Abbreviation = "BST", Name = "British Summer Time (British Standard Time from Feb 1968 to Oct 1971)", Offset = "UTC+01", OffsetHours = 1, OffsetMinutes = 0 },
                new TZone { Id = 136, Abbreviation = "CET", Name = "Central European Time", Offset = "UTC+01", OffsetHours = 1, OffsetMinutes = 0 },
                new TZone { Id = 137, Abbreviation = "DFT", Name = "AIX specific equivalent of Central European Time[6]", Offset = "UTC+01", OffsetHours = 1, OffsetMinutes = 0 },
                new TZone { Id = 138, Abbreviation = "IST", Name = "Irish Standard Time[7]", Offset = "UTC+01", OffsetHours = 1, OffsetMinutes = 0 },
                new TZone { Id = 139, Abbreviation = "MVT", Name = "Maldives Time", Offset = "UTC+05", OffsetHours = 5, OffsetMinutes = 0 },
                new TZone { Id = 140, Abbreviation = "MET", Name = "Middle European Time Same zone as CET", Offset = "UTC+01", OffsetHours = 1, OffsetMinutes = 0 },
                new TZone { Id = 141, Abbreviation = "WEDT", Name = "Western European Daylight Time", Offset = "UTC+01", OffsetHours = 1, OffsetMinutes = 0 },
                new TZone { Id = 142, Abbreviation = "WEST", Name = "Western European Summer Time", Offset = "UTC+01", OffsetHours = 1, OffsetMinutes = 0 },
                new TZone { Id = 143, Abbreviation = "CAT", Name = "Central Africa Time", Offset = "UTC+02", OffsetHours = 2, OffsetMinutes = 0 },
                new TZone { Id = 144, Abbreviation = "CEDT", Name = "Central European Daylight Time", Offset = "UTC+02", OffsetHours = 2, OffsetMinutes = 0 },
                new TZone { Id = 145, Abbreviation = "CEST", Name = "Central European Summer Time (Cf. HAEC)", Offset = "UTC+02", OffsetHours = 2, OffsetMinutes = 0 },
                new TZone { Id = 146, Abbreviation = "EET", Name = "Eastern European Time", Offset = "UTC+02", OffsetHours = 2, OffsetMinutes = 0 },
                new TZone { Id = 147, Abbreviation = "HAEC", Name = "Heure Avanc?e d'Europe Centrale francised name for CEST", Offset = "UTC+02", OffsetHours = 2, OffsetMinutes = 0 },
                new TZone { Id = 148, Abbreviation = "IST", Name = "Israel Standard Time", Offset = "UTC+02", OffsetHours = 2, OffsetMinutes = 0 },
                new TZone { Id = 149, Abbreviation = "MEST", Name = "Middle European Summer Time Same zone as CEST", Offset = "UTC+02", OffsetHours = 2, OffsetMinutes = 0 },
                new TZone { Id = 150, Abbreviation = "SAST", Name = "South African Standard Time", Offset = "UTC+02", OffsetHours = 2, OffsetMinutes = 0 },
                new TZone { Id = 151, Abbreviation = "WAT", Name = "West Africa Time", Offset = "UTC+01", OffsetHours = 1, OffsetMinutes = 0 },
                new TZone { Id = 152, Abbreviation = "ORAT", Name = "Oral Time", Offset = "UTC+05", OffsetHours = 5, OffsetMinutes = 0 },
                new TZone { Id = 153, Abbreviation = "PKT", Name = "Pakistan Standard Time", Offset = "UTC+05", OffsetHours = 5, OffsetMinutes = 0 },
                new TZone { Id = 154, Abbreviation = "TFT", Name = "Indian/Kerguelen", Offset = "UTC+05", OffsetHours = 5, OffsetMinutes = 0 },
                new TZone { Id = 155, Abbreviation = "BDT", Name = "Brunei Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 156, Abbreviation = "CHOT", Name = "Choibalsan", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 157, Abbreviation = "CIT", Name = "Central Indonesia Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 158, Abbreviation = "CST", Name = "China Standard Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 159, Abbreviation = "CT", Name = "China time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 160, Abbreviation = "HKT", Name = "Hong Kong Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 161, Abbreviation = "IRKT", Name = "Irkutsk Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 162, Abbreviation = "MST", Name = "Malaysia Standard Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 163, Abbreviation = "MYT", Name = "Malaysia Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 164, Abbreviation = "PST", Name = "Philippine Standard Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 165, Abbreviation = "AWST", Name = "Australian Western Standard Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 166, Abbreviation = "SGT", Name = "Singapore Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 167, Abbreviation = "ULAT", Name = "Ulaanbaatar Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 168, Abbreviation = "WST", Name = "Western Standard Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 169, Abbreviation = "CWST", Name = "Central Western Standard Time (Australia) unofficial", Offset = "UTC+08:45", OffsetHours = 8, OffsetMinutes = 45 },
                new TZone { Id = 170, Abbreviation = "AWDT", Name = "Australian Western Daylight Time", Offset = "UTC+09", OffsetHours = 9, OffsetMinutes = 0 },
                new TZone { Id = 171, Abbreviation = "EIT", Name = "Eastern Indonesian Time", Offset = "UTC+09", OffsetHours = 9, OffsetMinutes = 0 },
                new TZone { Id = 172, Abbreviation = "JST", Name = "Japan Standard Time", Offset = "UTC+09", OffsetHours = 9, OffsetMinutes = 0 },
                new TZone { Id = 173, Abbreviation = "KST", Name = "Korea Standard Time", Offset = "UTC+09", OffsetHours = 9, OffsetMinutes = 0 },
                new TZone { Id = 174, Abbreviation = "TLT", Name = "Timor Leste Time", Offset = "UTC+09", OffsetHours = 9, OffsetMinutes = 0 },
                new TZone { Id = 175, Abbreviation = "YAKT", Name = "Yakutsk Time", Offset = "UTC+09", OffsetHours = 9, OffsetMinutes = 0 },
                new TZone { Id = 176, Abbreviation = "ACST", Name = "Australian Central Standard Time", Offset = "UTC+09:30", OffsetHours = 9, OffsetMinutes = 30 },
                new TZone { Id = 177, Abbreviation = "SST", Name = "Singapore Standard Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 178, Abbreviation = "SST", Name = "Samoa Standard Time", Offset = "UTC-11", OffsetHours = -11, OffsetMinutes = 0 },
                new TZone { Id = 179, Abbreviation = "ACT", Name = "ASEAN Common Time", Offset = "UTC+08", OffsetHours = 8, OffsetMinutes = 0 },
                new TZone { Id = 180, Abbreviation = "THA", Name = "Thailand Standard Time", Offset = "UTC+07", OffsetHours = 7, OffsetMinutes = 0 },
                new TZone { Id = 181, Abbreviation = "TJT", Name = "Tajikistan Time", Offset = "UTC+05", OffsetHours = 5, OffsetMinutes = 0 },
                new TZone { Id = 182, Abbreviation = "TMT", Name = "Turkmenistan Time", Offset = "UTC+05", OffsetHours = 5, OffsetMinutes = 0 },
                new TZone { Id = 183, Abbreviation = "UZT", Name = "Uzbekistan Time", Offset = "UTC+05", OffsetHours = 5, OffsetMinutes = 0 },
                new TZone { Id = 184, Abbreviation = "YEKT", Name = "Yekaterinburg Time", Offset = "UTC+05", OffsetHours = 5, OffsetMinutes = 0 },
                new TZone { Id = 185, Abbreviation = "IST", Name = "Indian Standard Time", Offset = "UTC+05:30", OffsetHours = 5, OffsetMinutes = 30 },
                new TZone { Id = 186, Abbreviation = "SLST", Name = "Sri Lanka Standard Time", Offset = "UTC+05:30", OffsetHours = 5, OffsetMinutes = 30 },
                new TZone { Id = 187, Abbreviation = "NPT", Name = "Nepal Time", Offset = "UTC+05:45", OffsetHours = 5, OffsetMinutes = 45 },
                new TZone { Id = 188, Abbreviation = "BDT", Name = "Bangladesh Daylight Time (Bangladesh Daylight saving time keeps UTC+06 offset) [3]", Offset = "UTC+06", OffsetHours = 6, OffsetMinutes = 0 },
                new TZone { Id = 189, Abbreviation = "BIOT", Name = "British Indian Ocean Time", Offset = "UTC+06", OffsetHours = 6, OffsetMinutes = 0 },
                new TZone { Id = 190, Abbreviation = "BST", Name = "Bangladesh Standard Time", Offset = "UTC+06", OffsetHours = 6, OffsetMinutes = 0 },
                new TZone { Id = 191, Abbreviation = "WIT", Name = "Western Indonesian Time", Offset = "UTC+07", OffsetHours = 7, OffsetMinutes = 0 },
                new TZone { Id = 192, Abbreviation = "BTT", Name = "Bhutan Time", Offset = "UTC+06", OffsetHours = 6, OffsetMinutes = 0 },
                new TZone { Id = 193, Abbreviation = "OMST", Name = "Omsk Time", Offset = "UTC+06", OffsetHours = 6, OffsetMinutes = 0 },
                new TZone { Id = 194, Abbreviation = "VOST", Name = "Vostok Station Time", Offset = "UTC+06", OffsetHours = 6, OffsetMinutes = 0 },
                new TZone { Id = 195, Abbreviation = "CCT", Name = "Cocos Islands Time", Offset = "UTC+06:30", OffsetHours = 6, OffsetMinutes = 30 },
                new TZone { Id = 196, Abbreviation = "MMT", Name = "Myanmar Time", Offset = "UTC+06:30", OffsetHours = 6, OffsetMinutes = 30 },
                new TZone { Id = 197, Abbreviation = "MST", Name = "Myanmar Standard Time", Offset = "UTC+06:30", OffsetHours = 6, OffsetMinutes = 30 },
                new TZone { Id = 198, Abbreviation = "CXT", Name = "Christmas Island Time", Offset = "UTC+07", OffsetHours = 7, OffsetMinutes = 0 },
                new TZone { Id = 199, Abbreviation = "DAVT", Name = "Davis Time", Offset = "UTC+07", OffsetHours = 7, OffsetMinutes = 0 },
                new TZone { Id = 200, Abbreviation = "HOVT", Name = "Khovd Time", Offset = "UTC+07", OffsetHours = 7, OffsetMinutes = 0 },
                new TZone { Id = 201, Abbreviation = "ICT", Name = "Indochina Time", Offset = "UTC+07", OffsetHours = 7, OffsetMinutes = 0 },
                new TZone { Id = 202, Abbreviation = "KRAT", Name = "Krasnoyarsk Time", Offset = "UTC+07", OffsetHours = 7, OffsetMinutes = 0 },
                new TZone { Id = 203, Abbreviation = "KGT", Name = "Kyrgyzstan time", Offset = "UTC+06", OffsetHours = 6, OffsetMinutes = 0 },
                new TZone { Id = 204, Abbreviation = "BIT", Name = "Baker Island Time", Offset = "UTC-12", OffsetHours = -12, OffsetMinutes = 0 }
                );
        }
    }
}
