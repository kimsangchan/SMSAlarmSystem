// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 메시지 그룹 관련 비즈니스 로직을 정의하는 인터페이스
// 설명: 메시지 그룹 CRUD 및 관련 기능에 대한 계약을 정의합니다.

using SMSAlarmSystem.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Services.Interfaces
{
    /// <summary>
    /// 메시지 그룹 관련 비즈니스 로직을 정의하는 인터페이스
    /// 메시지 그룹 조회, 추가, 수정, 삭제 및 회원 관리 기능을 제공합니다.
    /// </summary>
    public interface IMessageGroupService
    {
        /// <summary>
        /// 모든 메시지 그룹을 조회합니다.
        /// </summary>
        /// <returns>메시지 그룹 목록</returns>
        Task<IEnumerable<MessageGroup>> GetAllGroupsAsync();

        /// <summary>
        /// 활성화된 메시지 그룹만 조회합니다.
        /// </summary>
        /// <returns>활성화된 메시지 그룹 목록</returns>
        Task<IEnumerable<MessageGroup>> GetActiveGroupsAsync();

        /// <summary>
        /// ID로 특정 메시지 그룹을 조회합니다.
        /// </summary>
        /// <param name="id">조회할 메시지 그룹 ID</param>
        /// <returns>조회된 메시지 그룹 또는 null(그룹이 없는 경우)</returns>
        Task<MessageGroup?> GetGroupByIdAsync(int id);

        /// <summary>
        /// 새 메시지 그룹을 추가합니다.
        /// </summary>
        /// <param name="group">추가할 메시지 그룹 객체</param>
        /// <returns>추가 성공 여부</returns>
        Task<bool> AddGroupAsync(MessageGroup group);

        /// <summary>
        /// 기존 메시지 그룹을 업데이트합니다.
        /// </summary>
        /// <param name="group">업데이트할 메시지 그룹 객체</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> UpdateGroupAsync(MessageGroup group);

        /// <summary>
        /// 메시지 그룹을 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 메시지 그룹 ID</param>
        /// <returns>삭제 성공 여부</returns>
        Task<bool> DeleteGroupAsync(int id);

        /// <summary>
        /// 메시지 그룹에 회원을 추가합니다.
        /// </summary>
        /// <param name="groupId">회원을 추가할 그룹 ID</param>
        /// <param name="memberId">추가할 회원 ID</param>
        /// <returns>회원 추가 성공 여부</returns>
        Task<bool> AddMemberToGroupAsync(int groupId, int memberId);

        /// <summary>
        /// 메시지 그룹에서 회원을 제거합니다.
        /// </summary>
        /// <param name="groupId">회원을 제거할 그룹 ID</param>
        /// <param name="memberId">제거할 회원 ID</param>
        /// <returns>회원 제거 성공 여부</returns>
        Task<bool> RemoveMemberFromGroupAsync(int groupId, int memberId);

        /// <summary>
        /// 특정 메시지 그룹에 속한 회원 목록을 조회합니다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <returns>그룹에 속한 회원 목록</returns>
        Task<IEnumerable<Member>> GetGroupMembersAsync(int groupId);

        /// <summary>
        /// 메시지 그룹의 활성화 상태를 전환합니다.
        /// </summary>
        /// <param name="id">상태를 변경할 그룹 ID</param>
        /// <returns>상태 변경 성공 여부</returns>
        Task<bool> ToggleGroupActiveStatusAsync(int id);
    }
}
