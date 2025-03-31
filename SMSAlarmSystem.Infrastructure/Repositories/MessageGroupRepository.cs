// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
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
    /// <summary>
    /// 메시지 그룹 데이터를 관리하는 리포지토리 클래스
    /// </summary>
    public class MessageGroupRepository : Repository<MessageGroup>, IMessageGroupRepository
    {
        // 데이터베이스 컨텍스트 (부모 클래스의 필드를 가리기 위해 new 키워드 사용)
        private new readonly ApplicationDbContext _context;

        // 로거 인스턴스 (부모 클래스의 필드를 가리기 위해 new 키워드 사용)
        private new readonly ILogger<MessageGroupRepository> _logger;

        /// <summary>
        /// MessageGroupRepository 생성자
        /// </summary>
        /// <param name="context">데이터베이스 컨텍스트</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        public MessageGroupRepository(ApplicationDbContext context, ILogger<MessageGroupRepository> logger)
            : base(context, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "데이터베이스 컨텍스트는 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");
        }

        /// <summary>
        /// 활성화된 메시지 그룹만 조회합니다.
        /// </summary>
        /// <returns>활성화된 메시지 그룹 목록</returns>
        public async Task<IEnumerable<MessageGroup>> GetActiveGroupsAsync()
        {
            try
            {
                _logger.LogInformation("활성화된 메시지 그룹 조회 시작");
                var groups = await _context.MessageGroups.Where(g => g.IsActive).ToListAsync();
                _logger.LogInformation("활성화된 메시지 그룹 조회 완료: {Count}개 그룹 조회됨", groups.Count);
                return groups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "활성화된 메시지 그룹 조회 중 오류 발생: {ErrorMessage}", ex.Message);
                throw; // 서비스 계층에서 예외 처리를 위해 예외를 다시 던짐
            }
        }

        /// <summary>
        /// 메시지 그룹에 회원을 추가합니다.
        /// </summary>
        /// <param name="groupId">회원을 추가할 그룹 ID</param>
        /// <param name="memberId">추가할 회원 ID</param>
        /// <exception cref="ArgumentException">유효하지 않은 그룹 ID 또는 회원 ID인 경우 발생</exception>
        public async Task AddMemberToGroupAsync(int groupId, int memberId)
        {
            // ID 유효성 검사
            if (groupId <= 0)
            {
                throw new ArgumentException("그룹 ID는 0보다 커야 합니다.", nameof(groupId));
            }

            if (memberId <= 0)
            {
                throw new ArgumentException("회원 ID는 0보다 커야 합니다.", nameof(memberId));
            }

            try
            {
                _logger.LogInformation("그룹에 회원 추가 시작: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);

                // 이미 그룹에 속해 있는지 확인
                var existingMember = await _context.MessageGroupMembers
                    .FirstOrDefaultAsync(gm => gm.MessageGroupId == groupId && gm.MemberId == memberId);

                if (existingMember != null)
                {
                    _logger.LogWarning("회원이 이미 그룹에 속해 있음: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);
                    return; // 이미 속해 있으면 아무 작업도 하지 않음
                }

                var groupMember = new MessageGroupMember { MessageGroupId = groupId, MemberId = memberId };
                await _context.MessageGroupMembers.AddAsync(groupMember);
                await _context.SaveChangesAsync();

                _logger.LogInformation("그룹에 회원 추가 완료: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "그룹에 회원 추가 중 데이터베이스 오류 발생: 그룹ID={GroupId}, 회원ID={MemberId}, 오류={ErrorMessage}",
                    groupId, memberId, ex.Message);
                throw;
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                _logger.LogError(ex, "그룹에 회원 추가 중 오류 발생: 그룹ID={GroupId}, 회원ID={MemberId}, 오류={ErrorMessage}",
                    groupId, memberId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 메시지 그룹에서 회원을 제거합니다.
        /// </summary>
        /// <param name="groupId">회원을 제거할 그룹 ID</param>
        /// <param name="memberId">제거할 회원 ID</param>
        /// <exception cref="ArgumentException">유효하지 않은 그룹 ID 또는 회원 ID인 경우 발생</exception>
        public async Task RemoveMemberFromGroupAsync(int groupId, int memberId)
        {
            // ID 유효성 검사
            if (groupId <= 0)
            {
                throw new ArgumentException("그룹 ID는 0보다 커야 합니다.", nameof(groupId));
            }

            if (memberId <= 0)
            {
                throw new ArgumentException("회원 ID는 0보다 커야 합니다.", nameof(memberId));
            }

            try
            {
                _logger.LogInformation("그룹에서 회원 제거 시작: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);

                var groupMember = await _context.MessageGroupMembers
                    .FirstOrDefaultAsync(gm => gm.MessageGroupId == groupId && gm.MemberId == memberId);

                if (groupMember == null)
                {
                    _logger.LogWarning("그룹에서 회원 제거 건너뜀: 회원이 그룹에 속해 있지 않음, 그룹ID={GroupId}, 회원ID={MemberId}",
                        groupId, memberId);
                    return; // 속해 있지 않으면 아무 작업도 하지 않음
                }

                _context.MessageGroupMembers.Remove(groupMember);
                await _context.SaveChangesAsync();

                _logger.LogInformation("그룹에서 회원 제거 완료: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "그룹에서 회원 제거 중 데이터베이스 오류 발생: 그룹ID={GroupId}, 회원ID={MemberId}, 오류={ErrorMessage}",
                    groupId, memberId, ex.Message);
                throw;
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                _logger.LogError(ex, "그룹에서 회원 제거 중 오류 발생: 그룹ID={GroupId}, 회원ID={MemberId}, 오류={ErrorMessage}",
                    groupId, memberId, ex.Message);
                throw;
            }
        }
    }
}
