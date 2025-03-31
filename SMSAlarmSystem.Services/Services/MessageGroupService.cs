// 작성자: Sangchan, Kim
// 작성일: 2025-03-31 (업데이트)
// 기능: 메시지 그룹 관련 비즈니스 로직을 처리하는 서비스
// 설명: 메시지 그룹 CRUD 및 관련 기능을 제공합니다.
//       그룹 생성, 수정, 삭제 및 그룹 멤버 관리 기능을 포함합니다.

using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Services.Services
{
    /// <summary>
    /// 메시지 그룹 관련 비즈니스 로직을 처리하는 서비스 클래스
    /// 메시지 그룹 관리 및 그룹 멤버 관리 기능을 제공합니다.
    /// </summary>
    public class MessageGroupService : IMessageGroupService
    {
        // 메시지 그룹 데이터 접근을 위한 리포지토리
        private readonly IMessageGroupRepository _messageGroupRepository;

        // 회원 데이터 접근을 위한 리포지토리
        private readonly IMemberRepository _memberRepository;

        // 로깅을 위한 로거 인스턴스
        private readonly ILogger<MessageGroupService> _logger;

        /// <summary>
        /// MessageGroupService 생성자
        /// </summary>
        /// <param name="messageGroupRepository">메시지 그룹 리포지토리 인스턴스</param>
        /// <param name="memberRepository">회원 리포지토리 인스턴스</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public MessageGroupService(
            IMessageGroupRepository messageGroupRepository,
            IMemberRepository memberRepository,
            ILogger<MessageGroupService> logger)
        {
            // null 체크 및 예외 처리
            _messageGroupRepository = messageGroupRepository ?? throw new ArgumentNullException(nameof(messageGroupRepository), "메시지 그룹 리포지토리는 null이 될 수 없습니다.");
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository), "회원 리포지토리는 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");

            _logger.LogInformation("MessageGroupService 초기화 완료");
        }

        /// <summary>
        /// 모든 메시지 그룹을 조회합니다.
        /// </summary>
        /// <returns>메시지 그룹 목록 또는 오류 발생 시 빈 목록</returns>
        public async Task<IEnumerable<MessageGroup>> GetAllGroupsAsync()
        {
            try
            {
                _logger.LogInformation("모든 메시지 그룹 조회 시작");
                var groups = await _messageGroupRepository.GetAllAsync();

                // null 체크 (방어적 프로그래밍)
                if (groups == null)
                {
                    _logger.LogWarning("리포지토리에서 null 반환됨. 빈 목록으로 대체합니다.");
                    return new List<MessageGroup>();
                }

                _logger.LogInformation("모든 메시지 그룹 조회 완료: {Count}개 조회됨", groups.Count());
                return groups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "모든 메시지 그룹 조회 중 오류 발생: {ErrorMessage}", ex.Message);
                return new List<MessageGroup>();
            }
        }

        /// <summary>
        /// 활성화된 메시지 그룹만 조회합니다.
        /// </summary>
        /// <returns>활성화된 메시지 그룹 목록 또는 오류 발생 시 빈 목록</returns>
        public async Task<IEnumerable<MessageGroup>> GetActiveGroupsAsync()
        {
            try
            {
                _logger.LogInformation("활성화된 메시지 그룹 조회 시작");
                var groups = await _messageGroupRepository.GetActiveGroupsAsync();

                // null 체크 (방어적 프로그래밍)
                if (groups == null)
                {
                    _logger.LogWarning("리포지토리에서 null 반환됨. 빈 목록으로 대체합니다.");
                    return new List<MessageGroup>();
                }

                _logger.LogInformation("활성화된 메시지 그룹 조회 완료: {Count}개 조회됨", groups.Count());
                return groups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "활성화된 메시지 그룹 조회 중 오류 발생: {ErrorMessage}", ex.Message);
                return new List<MessageGroup>();
            }
        }

        /// <summary>
        /// ID로 특정 메시지 그룹을 조회합니다.
        /// </summary>
        /// <param name="id">조회할 메시지 그룹 ID</param>
        /// <returns>조회된 메시지 그룹 또는 null(그룹이 없거나 오류 발생 시)</returns>
        public async Task<MessageGroup?> GetGroupByIdAsync(int id)
        {
            // ID 유효성 검사
            if (id <= 0)
            {
                _logger.LogWarning("메시지 그룹 조회 실패: 유효하지 않은 ID={Id}", id);
                return null;
            }

            try
            {
                _logger.LogInformation("메시지 그룹 조회 시작: ID={Id}", id);
                var group = await _messageGroupRepository.GetByIdAsync(id);

                if (group == null)
                {
                    _logger.LogWarning("메시지 그룹을 찾을 수 없음: ID={Id}", id);
                }
                else
                {
                    _logger.LogInformation("메시지 그룹 조회 완료: ID={Id}, 이름={Name}", id, group.Name);
                }

                return group;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 그룹 조회 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 새 메시지 그룹을 추가합니다.
        /// </summary>
        /// <param name="group">추가할 메시지 그룹 객체</param>
        /// <returns>추가 성공 여부</returns>
        public async Task<bool> AddGroupAsync(MessageGroup group)
        {
            // null 체크
            if (group == null)
            {
                _logger.LogError("메시지 그룹 추가 실패: 그룹 객체가 null입니다.");
                return false;
            }

            // 그룹 이름 유효성 검사
            if (string.IsNullOrWhiteSpace(group.Name))
            {
                _logger.LogError("메시지 그룹 추가 실패: 그룹 이름이 비어 있습니다.");
                return false;
            }

            try
            {
                _logger.LogInformation("메시지 그룹 추가 시작: 이름={Name}", group.Name);
                await _messageGroupRepository.AddAsync(group);
                _logger.LogInformation("메시지 그룹 추가 완료: ID={Id}, 이름={Name}", group.Id, group.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 그룹 추가 중 오류 발생: 이름={Name}, 오류={ErrorMessage}", group.Name, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 기존 메시지 그룹을 업데이트합니다.
        /// </summary>
        /// <param name="group">업데이트할 메시지 그룹 객체</param>
        /// <returns>업데이트 성공 여부</returns>
        public async Task<bool> UpdateGroupAsync(MessageGroup group)
        {
            // null 체크
            if (group == null)
            {
                _logger.LogError("메시지 그룹 업데이트 실패: 그룹 객체가 null입니다.");
                return false;
            }

            // ID 유효성 검사
            if (group.Id <= 0)
            {
                _logger.LogError("메시지 그룹 업데이트 실패: 유효하지 않은 ID={Id}", group.Id);
                return false;
            }

            // 그룹 이름 유효성 검사
            if (string.IsNullOrWhiteSpace(group.Name))
            {
                _logger.LogError("메시지 그룹 업데이트 실패: 그룹 이름이 비어 있습니다. ID={Id}", group.Id);
                return false;
            }

            try
            {
                // 그룹 존재 여부 확인
                var existingGroup = await _messageGroupRepository.GetByIdAsync(group.Id);
                if (existingGroup == null)
                {
                    _logger.LogWarning("메시지 그룹 업데이트 실패: 그룹을 찾을 수 없음, ID={Id}", group.Id);
                    return false;
                }

                _logger.LogInformation("메시지 그룹 업데이트 시작: ID={Id}, 이름={Name}", group.Id, group.Name);
                await _messageGroupRepository.UpdateAsync(group);
                _logger.LogInformation("메시지 그룹 업데이트 완료: ID={Id}, 이름={Name}", group.Id, group.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 그룹 업데이트 중 오류 발생: ID={Id}, 이름={Name}, 오류={ErrorMessage}",
                    group.Id, group.Name, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 메시지 그룹을 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 메시지 그룹 ID</param>
        /// <returns>삭제 성공 여부</returns>
        public async Task<bool> DeleteGroupAsync(int id)
        {
            // ID 유효성 검사
            if (id <= 0)
            {
                _logger.LogError("메시지 그룹 삭제 실패: 유효하지 않은 ID={Id}", id);
                return false;
            }

            try
            {
                _logger.LogInformation("메시지 그룹 삭제 시작: ID={Id}", id);
                var group = await _messageGroupRepository.GetByIdAsync(id);

                if (group == null)
                {
                    _logger.LogWarning("메시지 그룹 삭제 실패: 그룹을 찾을 수 없음, ID={Id}", id);
                    return false;
                }

                await _messageGroupRepository.DeleteAsync(group);
                _logger.LogInformation("메시지 그룹 삭제 완료: ID={Id}, 이름={Name}", id, group.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 그룹 삭제 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 그룹에 회원을 추가합니다.
        /// </summary>
        /// <param name="groupId">회원을 추가할 그룹 ID</param>
        /// <param name="memberId">추가할 회원 ID</param>
        /// <returns>회원 추가 성공 여부</returns>
        public async Task<bool> AddMemberToGroupAsync(int groupId, int memberId)
        {
            // ID 유효성 검사
            if (groupId <= 0)
            {
                _logger.LogError("그룹에 회원 추가 실패: 유효하지 않은 그룹 ID={GroupId}", groupId);
                return false;
            }

            if (memberId <= 0)
            {
                _logger.LogError("그룹에 회원 추가 실패: 유효하지 않은 회원 ID={MemberId}", memberId);
                return false;
            }

            try
            {
                _logger.LogInformation("그룹에 회원 추가 시작: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);

                // 그룹 존재 여부 확인
                var group = await _messageGroupRepository.GetByIdAsync(groupId);
                if (group == null)
                {
                    _logger.LogWarning("그룹에 회원 추가 실패: 그룹을 찾을 수 없음, 그룹ID={GroupId}", groupId);
                    return false;
                }

                // 회원 존재 여부 확인
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    _logger.LogWarning("그룹에 회원 추가 실패: 회원을 찾을 수 없음, 회원ID={MemberId}", memberId);
                    return false;
                }

                // 이미 그룹에 속해 있는지 확인 (선택적)
                var groupMembers = await _memberRepository.GetMembersByGroupIdAsync(groupId);
                if (groupMembers.Any(m => m.Id == memberId))
                {
                    _logger.LogInformation("그룹에 회원 추가 건너뜀: 회원이 이미 그룹에 속해 있음, 그룹ID={GroupId}, 회원ID={MemberId}",
                        groupId, memberId);
                    return true; // 이미 속해 있으므로 성공으로 간주
                }

                await _messageGroupRepository.AddMemberToGroupAsync(groupId, memberId);
                _logger.LogInformation("그룹에 회원 추가 완료: 그룹ID={GroupId}, 그룹명={GroupName}, 회원ID={MemberId}, 회원명={MemberName}",
                    groupId, group.Name, memberId, member.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹에 회원 추가 중 오류 발생: 그룹ID={GroupId}, 회원ID={MemberId}, 오류={ErrorMessage}",
                    groupId, memberId, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 그룹에서 회원을 제거합니다.
        /// </summary>
        /// <param name="groupId">회원을 제거할 그룹 ID</param>
        /// <param name="memberId">제거할 회원 ID</param>
        /// <returns>회원 제거 성공 여부</returns>
        public async Task<bool> RemoveMemberFromGroupAsync(int groupId, int memberId)
        {
            // ID 유효성 검사
            if (groupId <= 0)
            {
                _logger.LogError("그룹에서 회원 제거 실패: 유효하지 않은 그룹 ID={GroupId}", groupId);
                return false;
            }

            if (memberId <= 0)
            {
                _logger.LogError("그룹에서 회원 제거 실패: 유효하지 않은 회원 ID={MemberId}", memberId);
                return false;
            }

            try
            {
                _logger.LogInformation("그룹에서 회원 제거 시작: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);

                // 그룹 존재 여부 확인
                var group = await _messageGroupRepository.GetByIdAsync(groupId);
                if (group == null)
                {
                    _logger.LogWarning("그룹에서 회원 제거 실패: 그룹을 찾을 수 없음, 그룹ID={GroupId}", groupId);
                    return false;
                }

                // 회원 존재 여부 확인
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    _logger.LogWarning("그룹에서 회원 제거 실패: 회원을 찾을 수 없음, 회원ID={MemberId}", memberId);
                    return false;
                }

                // 회원이 그룹에 속해 있는지 확인 (선택적)
                var groupMembers = await _memberRepository.GetMembersByGroupIdAsync(groupId);
                if (!groupMembers.Any(m => m.Id == memberId))
                {
                    _logger.LogInformation("그룹에서 회원 제거 건너뜀: 회원이 그룹에 속해 있지 않음, 그룹ID={GroupId}, 회원ID={MemberId}",
                        groupId, memberId);
                    return true; // 이미 제거되어 있으므로 성공으로 간주
                }

                await _messageGroupRepository.RemoveMemberFromGroupAsync(groupId, memberId);
                _logger.LogInformation("그룹에서 회원 제거 완료: 그룹ID={GroupId}, 그룹명={GroupName}, 회원ID={MemberId}, 회원명={MemberName}",
                    groupId, group.Name, memberId, member.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹에서 회원 제거 중 오류 발생: 그룹ID={GroupId}, 회원ID={MemberId}, 오류={ErrorMessage}",
                    groupId, memberId, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 그룹에 속한 회원 목록을 조회합니다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <returns>그룹에 속한 회원 목록</returns>
        public async Task<IEnumerable<Member>> GetGroupMembersAsync(int groupId)
        {
            // ID 유효성 검사
            if (groupId <= 0)
            {
                _logger.LogError("그룹 회원 목록 조회 실패: 유효하지 않은 그룹 ID={GroupId}", groupId);
                return new List<Member>();
            }

            try
            {
                _logger.LogInformation("그룹 회원 목록 조회 시작: 그룹ID={GroupId}", groupId);

                // 그룹 존재 여부 확인
                var group = await _messageGroupRepository.GetByIdAsync(groupId);
                if (group == null)
                {
                    _logger.LogWarning("그룹 회원 목록 조회 실패: 그룹을 찾을 수 없음, 그룹ID={GroupId}", groupId);
                    return new List<Member>();
                }

                var members = await _memberRepository.GetMembersByGroupIdAsync(groupId);

                // null 체크 (방어적 프로그래밍)
                if (members == null)
                {
                    _logger.LogWarning("리포지토리에서 null 반환됨. 빈 목록으로 대체합니다.");
                    return new List<Member>();
                }

                _logger.LogInformation("그룹 회원 목록 조회 완료: 그룹ID={GroupId}, 그룹명={GroupName}, 회원 수={MemberCount}",
                    groupId, group.Name, members.Count());
                return members;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹 회원 목록 조회 중 오류 발생: 그룹ID={GroupId}, 오류={ErrorMessage}",
                    groupId, ex.Message);
                return new List<Member>();
            }
        }

        /// <summary>
        /// 그룹의 활성화 상태를 전환합니다.
        /// </summary>
        /// <param name="id">상태를 변경할 그룹 ID</param>
        /// <returns>상태 변경 성공 여부</returns>
        public async Task<bool> ToggleGroupActiveStatusAsync(int id)
        {
            // ID 유효성 검사
            if (id <= 0)
            {
                _logger.LogError("그룹 활성화 상태 변경 실패: 유효하지 않은 ID={Id}", id);
                return false;
            }

            try
            {
                _logger.LogInformation("그룹 활성화 상태 변경 시작: ID={Id}", id);
                var group = await _messageGroupRepository.GetByIdAsync(id);

                if (group == null)
                {
                    _logger.LogWarning("그룹 활성화 상태 변경 실패: 그룹을 찾을 수 없음, ID={Id}", id);
                    return false;
                }

                // 상태 변경
                var previousStatus = group.IsActive;
                group.IsActive = !group.IsActive;

                // 업데이트 시간 설정 (MessageGroup에 UpdatedAt 속성이 있는 경우)
                if (group.GetType().GetProperty("UpdatedAt") != null)
                {
                    // 업데이트 시간 설정 (MessageGroup에 UpdatedAt 속성이 있는 경우)
                    var updatedAtProperty = group.GetType().GetProperty("UpdatedAt");
                    updatedAtProperty?.SetValue(group, DateTime.UtcNow);
                }

                await _messageGroupRepository.UpdateAsync(group);
                _logger.LogInformation("그룹 활성화 상태 변경 완료: ID={Id}, 이름={Name}, 이전 상태={PreviousStatus}, 현재 상태={CurrentStatus}",
                    id, group.Name, previousStatus ? "활성" : "비활성", group.IsActive ? "활성" : "비활성");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹 활성화 상태 변경 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);
                return false;
            }
        }
    }
}
