// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 데이터베이스 컨텍스트 클래스
// 설명: 이 클래스는 Entity Framework Core를 사용하여 데이터베이스와의 연결을 관리합니다.
using Microsoft.EntityFrameworkCore;
using SMSAlarmSystem.Core.Models;

namespace SMSAlarmSystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 데이터베이스 테이블 정의
        public DbSet<Member> Members { get; set; } = null!;
        public DbSet<MessageGroup> MessageGroups { get; set; } = null!;
        public DbSet<AlarmPoint> AlarmPoints { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<ErrorLog> ErrorLogs { get; set; } = null!;
        public DbSet<MessageGroupMember> MessageGroupMembers { get; set; } = null!;

        // 모델 구성 설정
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 다대다 관계 설정 (MessageGroup과 Member)
            modelBuilder.Entity<MessageGroupMember>()
                .HasKey(mgm => new { mgm.MessageGroupId, mgm.MemberId });

            modelBuilder.Entity<MessageGroupMember>()
                .HasOne(mgm => mgm.MessageGroup)
                .WithMany(mg => mg.GroupMembers)
                .HasForeignKey(mgm => mgm.MessageGroupId);

            modelBuilder.Entity<MessageGroupMember>()
                .HasOne(mgm => mgm.Member)
                .WithMany(m => m.MessageGroupMembers)
                .HasForeignKey(mgm => mgm.MemberId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
