// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 메시지 그룹 데이터를 관리하는 리포지토리 클래스
// 설명: 이 클래스는 데이터베이스와 상호작용하여 메시지 그룹 데이터를 CRUD 방식으로 관리합니다.
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
    public class MessageGroupRepository : Repository<MessageGroup>, IMessageGroupRepository
    {
        private new readonly ApplicationDbContext _context;
        private new readonly ILogger<MessageGroupRepository> _logger;

        public MessageGroupRepository(ApplicationDbContext context, ILogger<MessageGroupRepository> logger)
            : base(context, logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<MessageGroup>> GetActiveGroupsAsync()
        {
            try
            {
                _logger.LogDebug("활성화된 메시지 그룹 조회 시작");
                return await _context.MessageGroups.Where(g => g.IsActive).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "활성화된 메시지 그룹 조회 중 오류 발생");
                return new List<MessageGroup>();
            }
        }

        public async Task AddMemberToGroupAsync(int groupId, int memberId)
        {
            try
            {
                _logger.LogDebug($"그룹에 회원 추가 시작: 그룹 ID {groupId}, 회원 ID {memberId}");
                var groupMember = new MessageGroupMember { MessageGroupId = groupId, MemberId = memberId };
                await _context.MessageGroupMembers.AddAsync(groupMember);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"그룹에 회원 추가 중 오류 발생: 그룹 ID {groupId}, 회원 ID {memberId}");
                throw;
            }
        }

        public async Task RemoveMemberFromGroupAsync(int groupId, int memberId)
        {
            try
            {
                _logger.LogDebug($"그룹에서 회원 제거 시작: 그룹 ID {groupId}, 회원 ID {memberId}");
                var groupMember = await _context.MessageGroupMembers
                    .FirstOrDefaultAsync(gm => gm.MessageGroupId == groupId && gm.MemberId == memberId);
                if (groupMember != null)
                {
                    _context.MessageGroupMembers.Remove(groupMember);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"그룹에서 회원 제거 중 오류 발생: 그룹 ID {groupId}, 회원 ID {memberId}");
                throw;
            }
        }
    }
}
