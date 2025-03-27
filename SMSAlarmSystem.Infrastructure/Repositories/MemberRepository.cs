// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 회원 데이터를 관리하는 리포지토리 클래스
// 설명: 이 클래스는 데이터베이스와 상호작용하여 회원 데이터를 CRUD 방식으로 관리합니다.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Infrastructure.Repositories
{
    public class MemberRepository : Repository<Member>, IMemberRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MemberRepository> _logger;

        public MemberRepository(ApplicationDbContext context, ILogger<MemberRepository> logger)
            : base(context, logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Member?> GetByPhoneNumberAsync(string phoneNumber)
        {
            try
            {
                _logger.LogDebug($"전화번호로 회원 조회 시작: {phoneNumber}");
                return await _context.Members.FirstOrDefaultAsync(m => m.PhoneNumber == phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"전화번호로 회원 조회 중 오류 발생: {phoneNumber}");
                return null;
            }
        }

        public async Task<IEnumerable<Member>> GetMembersByGroupIdAsync(int groupId)
        {
            try
            {
                _logger.LogDebug($"그룹 ID로 회원 목록 조회 시작: {groupId}");
                return await _context.MessageGroupMembers
                    .Where(mgm => mgm.MessageGroupId == groupId)
                    .Select(mgm => mgm.Member)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"그룹 ID로 회원 목록 조회 중 오류 발생: {groupId}");
                return new List<Member>();
            }
        }
    }
}
