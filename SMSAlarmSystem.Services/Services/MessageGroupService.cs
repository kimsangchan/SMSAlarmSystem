// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 메시지 그룹 관련 비즈니스 로직을 처리하는 서비스
// 설명: 메시지 그룹 CRUD 및 관련 기능을 제공합니다.
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Services.Services
{
    public class MessageGroupService
    {
        private readonly IMessageGroupRepository _messageGroupRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly ILogger<MessageGroupService> _logger;

        public MessageGroupService(
            IMessageGroupRepository messageGroupRepository,
            IMemberRepository memberRepository,
            ILogger<MessageGroupService> logger)
        {
            _messageGroupRepository = messageGroupRepository ?? throw new ArgumentNullException(nameof(messageGroupRepository));
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 모든 메시지 그룹 가져오기
        public async Task<IEnumerable<MessageGroup>> GetAllGroupsAsync()
        {
            try
            {
                _logger.LogInformation("모든 메시지 그룹 조회 시작");
                var groups = await _messageGroupRepository.GetAllAsync();
                _logger.LogInformation("모든 메시지 그룹 조회 완료: {Count}개 조회됨", groups.Count());
                return groups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "모든 메시지 그룹 조회 중 오류 발생");
                return new List<MessageGroup>();
            }
        }

        // 활성화된 메시지 그룹만 가져오기
        public async Task<IEnumerable<MessageGroup>> GetActiveGroupsAsync()
        {
            try
            {
                _logger.LogInformation("활성화된 메시지 그룹 조회 시작");
                var groups = await _messageGroupRepository.GetActiveGroupsAsync();
                _logger.LogInformation("활성화된 메시지 그룹 조회 완료: {Count}개 조회됨", groups.Count());
                return groups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "활성화된 메시지 그룹 조회 중 오류 발생");
                return new List<MessageGroup>();
            }
        }

        // ID로 메시지 그룹 가져오기
        public async Task<MessageGroup?> GetGroupByIdAsync(int id)
        {
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
                _logger.LogError(ex, "메시지 그룹 조회 중 오류 발생: ID={Id}", id);
                return null;
            }
        }

        // 메시지 그룹 추가
        public async Task<bool> AddGroupAsync(MessageGroup group)
        {
            if (group == null)
            {
                _logger.LogError("메시지 그룹 추가 실패: 그룹 객체가 null입니다.");
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
                _logger.LogError(ex, "메시지 그룹 추가 중 오류 발생: 이름={Name}", group.Name);
                return false;
            }
        }

        // 메시지 그룹 업데이트
        public async Task<bool> UpdateGroupAsync(MessageGroup group)
        {
            if (group == null)
            {
                _logger.LogError("메시지 그룹 업데이트 실패: 그룹 객체가 null입니다.");
                return false;
            }

            try
            {
                _logger.LogInformation("메시지 그룹 업데이트 시작: ID={Id}, 이름={Name}", group.Id, group.Name);
                await _messageGroupRepository.UpdateAsync(group);
                _logger.LogInformation("메시지 그룹 업데이트 완료: ID={Id}, 이름={Name}", group.Id, group.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 그룹 업데이트 중 오류 발생: ID={Id}, 이름={Name}", group.Id, group.Name);
                return false;
            }
        }

        // 메시지 그룹 삭제
        public async Task<bool> DeleteGroupAsync(int id)
        {
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
                _logger.LogError(ex, "메시지 그룹 삭제 중 오류 발생: ID={Id}", id);
                return false;
            }
        }

        // 그룹에 회원 추가
        public async Task<bool> AddMemberToGroupAsync(int groupId, int memberId)
        {
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

                await _messageGroupRepository.AddMemberToGroupAsync(groupId, memberId);
                _logger.LogInformation("그룹에 회원 추가 완료: 그룹ID={GroupId}, 그룹명={GroupName}, 회원ID={MemberId}, 회원명={MemberName}",
                    groupId, group.Name, memberId, member.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹에 회원 추가 중 오류 발생: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);
                return false;
            }
        }

        // 그룹에서 회원 제거
        public async Task<bool> RemoveMemberFromGroupAsync(int groupId, int memberId)
        {
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

                await _messageGroupRepository.RemoveMemberFromGroupAsync(groupId, memberId);
                _logger.LogInformation("그룹에서 회원 제거 완료: 그룹ID={GroupId}, 그룹명={GroupName}, 회원ID={MemberId}, 회원명={MemberName}",
                    groupId, group.Name, memberId, member.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹에서 회원 제거 중 오류 발생: 그룹ID={GroupId}, 회원ID={MemberId}", groupId, memberId);
                return false;
            }
        }

        // 그룹에 속한 회원 목록 가져오기
        public async Task<IEnumerable<Member>> GetGroupMembersAsync(int groupId)
        {
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
                _logger.LogInformation("그룹 회원 목록 조회 완료: 그룹ID={GroupId}, 그룹명={GroupName}, 회원 수={MemberCount}",
                    groupId, group.Name, members.Count());
                return members;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹 회원 목록 조회 중 오류 발생: 그룹ID={GroupId}", groupId);
                return new List<Member>();
            }
        }

        // 그룹 활성화/비활성화 상태 변경
        public async Task<bool> ToggleGroupActiveStatusAsync(int id)
        {
            try
            {
                _logger.LogInformation("그룹 활성화 상태 변경 시작: ID={Id}", id);
                var group = await _messageGroupRepository.GetByIdAsync(id);

                if (group == null)
                {
                    _logger.LogWarning("그룹 활성화 상태 변경 실패: 그룹을 찾을 수 없음, ID={Id}", id);
                    return false;
                }

                group.IsActive = !group.IsActive;
                await _messageGroupRepository.UpdateAsync(group);
                _logger.LogInformation("그룹 활성화 상태 변경 완료: ID={Id}, 이름={Name}, 상태={Status}",
                    id, group.Name, group.IsActive ? "활성" : "비활성");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹 활성화 상태 변경 중 오류 발생: ID={Id}", id);
                return false;
            }
        }
    }
}
