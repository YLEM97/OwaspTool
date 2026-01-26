using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OwaspTool.Models.Database;

namespace OwaspTool.DAL;

public partial class OwaspToolContext : DbContext
{
    public OwaspToolContext()
    {
    }

    public OwaspToolContext(DbContextOptions<OwaspToolContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ASVSReqAnswer> ASVSReqAnswers { get; set; }

    public virtual DbSet<ASVSReqLevel> ASVSReqLevels { get; set; }

    public virtual DbSet<ASVSRequirement> ASVSRequirements { get; set; }

    public virtual DbSet<ASVSRequirementStatus> ASVSRequirementStatus { get; set; }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<AnswerOption> AnswerOptions { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Chapter> Chapters { get; set; }

    public virtual DbSet<GivenAnswer> GivenAnswers { get; set; }

    public virtual DbSet<Level> Levels { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Survey> Surveys { get; set; }

    public virtual DbSet<SurveyCategoryStatus> SurveyCategoryStatuses { get; set; }

    public virtual DbSet<SurveyInstance> SurveyInstances { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserWebApp> UserWebApps { get; set; }

    public virtual DbSet<Models.Database.WebApplication> WebApplications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ASVSReqAnswer>(entity =>
        {
            entity.HasKey(e => e.ASVSReqAnswerID).HasName("PK__ASVSReqA__B175C7ACC7889E67");

            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

            entity.HasOne(d => d.ASVSReqLevel).WithMany(p => p.ASVSReqAnswers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ASVSReqAn__ASVSR__72C60C4A");

            entity.HasOne(d => d.AnswerOption).WithMany(p => p.ASVSReqAnswers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ASVSReqAn__Answe__73BA3083");
        });

        modelBuilder.Entity<ASVSReqLevel>(entity =>
        {
            entity.HasKey(e => e.ASVSReqLevelID).HasName("PK__ASVSReqL__033C98CA917DFCF9");

            entity.Property(e => e.Active).HasDefaultValue(true);

            entity.HasOne(d => d.ASVSRequirement).WithMany(p => p.ASVSReqLevels)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ASVSReqLe__ASVSR__6E01572D");

            entity.HasOne(d => d.Level).WithMany(p => p.ASVSReqLevels)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ASVSReqLe__Level__6EF57B66");
        });

        modelBuilder.Entity<ASVSRequirement>(entity =>
        {
            entity.HasKey(e => e.ASVSRequirementID).HasName("PK__ASVSRequ__0E8494036DE61578");

            entity.HasOne(d => d.Chapter).WithMany(p => p.ASVSRequirements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ASVSRequi__Chapt__693CA210");

            entity.HasOne(d => d.Section).WithMany(p => p.ASVSRequirements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ASVSRequi__Secti__6A30C649");
        });

        modelBuilder.Entity<ASVSRequirementStatus>(entity =>
        {
            entity.HasKey(e => e.ASVSRequirementStatusID).HasName("PK__ASVSReqS__XXXXXXXXXXXXX");

            entity.Property(e => e.Modified).HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Notes)
                .HasColumnType("nvarchar(max)")
                .IsUnicode(true)
                .IsRequired(false);

            // Mapping per nuovo campo AiNotes
            entity.Property(e => e.AiNotes)
                .HasColumnType("nvarchar(max)")
                .IsUnicode(true)
                .IsRequired(false);

            entity.HasOne(d => d.UserWebApp).WithMany()
                .HasForeignKey(d => d.UserWebAppID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ASVSReqSt__UserW__XXXXXX");

            entity.HasOne(d => d.ASVSRequirement).WithMany()
                .HasForeignKey(d => d.ASVSRequirementID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ASVSReqSt__ASVSR__XXXXXX");
        });

        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerID).HasName("PK__Answer__D48250241B342D8E");

            entity.Property(e => e.Text).IsFixedLength();
        });

        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.HasKey(e => e.AnswerOptionID).HasName("PK__AnswerOp__CEB73996917FEDCB");

            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

            entity.HasOne(d => d.Answer).WithMany(p => p.AnswerOptions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AnswerOpt__Answe__5535A963");

            entity.HasOne(d => d.GoToQuestion).WithMany(p => p.AnswerOptionGoToQuestions).HasConstraintName("FK__AnswerOpt__GoToQ__5629CD9C");

            entity.HasOne(d => d.Question).WithMany(p => p.AnswerOptionQuestions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AnswerOpt__Quest__5441852A");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryID).HasName("PK__Category__19093A2BA6A9D43B");

            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

            entity.HasOne(d => d.Survey).WithMany(p => p.Categories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Category__Survey__4AB81AF0");
        });

        modelBuilder.Entity<Chapter>(entity =>
        {
            entity.HasKey(e => e.ChapterID).HasName("PK__Chapter__0893A34A70945350");
        });

        modelBuilder.Entity<GivenAnswer>(entity =>
        {
            entity.HasKey(e => e.GivenAnswerID).HasName("PK__GivenAns__36DB8C0DAA3891F4");

            entity.Property(e => e.Date).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.Modified).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.AnswerOption).WithMany(p => p.GivenAnswers).HasConstraintName("FK__GivenAnsw__Answe__5BE2A6F2");

            entity.HasOne(d => d.SurveyInstance).WithMany(p => p.GivenAnswers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GivenAnsw__Surve__5AEE82B9");
        });

        modelBuilder.Entity<Level>(entity =>
        {
            entity.HasKey(e => e.LevelID).HasName("PK__Level__09F03C063555E142");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionID).HasName("PK__Question__0DC06F8CE06EC61F");

            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

            entity.HasOne(d => d.Category).WithMany(p => p.Questions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Question__Catego__4E88ABD4");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.SectionID).HasName("PK__Section__80EF089236DD18BE");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusID).HasName("PK__Status__C8EE2043685D38B1");
        });

        modelBuilder.Entity<Survey>(entity =>
        {
            entity.HasKey(e => e.SurveyID).HasName("PK__Survey__A5481F9DEEA7B9E5");
        });

        modelBuilder.Entity<SurveyCategoryStatus>(entity =>
        {
            entity.HasKey(e => e.SurveyCategoryStatusID).HasName("PK__SurveyCa__174319BD7EEF7B96");

            entity.HasOne(d => d.Category).WithMany(p => p.SurveyCategoryStatuses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SurveyCat__Categ__619B8048");

            entity.HasOne(d => d.Status).WithMany(p => p.SurveyCategoryStatuses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SurveyCat__Statu__628FA481");

            entity.HasOne(d => d.SurveyInstance).WithMany(p => p.SurveyCategoryStatuses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SurveyCat__Surve__60A75C0F");
        });

        modelBuilder.Entity<SurveyInstance>(entity =>
        {
            entity.HasKey(e => e.SurveyInstanceID).HasName("PK__SurveyIn__17D575E76B1E752C");

            entity.HasOne(d => d.Survey).WithMany(p => p.SurveyInstances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SurveyIns__Surve__46E78A0C");

            entity.HasOne(d => d.UserWebApp).WithMany(p => p.SurveyInstances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SurveyIns__UserW__45F365D3");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserID).HasName("PK__User__1788CCAC23126163");

            entity.Property(e => e.UserID).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<UserWebApp>(entity =>
        {
            entity.HasKey(e => e.UserWebAppID).HasName("PK__UserWebA__A55D565EAC6E88D8");

            entity.HasOne(d => d.Level).WithMany(p => p.UserWebApps).HasConstraintName("FK__UserWebAp__Level__412EB0B6");

            entity.HasOne(d => d.User).WithMany(p => p.UserWebApps)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserWebAp__UserI__3F466844");

            entity.HasOne(d => d.WebApplication).WithMany(p => p.UserWebApps)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserWebAp__WebAp__403A8C7D");
        });

        modelBuilder.Entity<Models.Database.WebApplication>(entity =>
        {
            entity.HasKey(e => e.WebApplicationID).HasName("PK__WebAppli__00F29777FB5ED192");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
