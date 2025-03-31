// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 메시지 그룹 데이터 접근을 위한 인터페이스
// 설명: 메시지 그룹 관련 데이터 접근 메서드를 정의합니다.

using SMSAlarmSystem.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Core.Interfaces
{
    /// <summary>
    /// 메시지 그룹 데이터 접근을 위한 인터페이스
    /// 기본 CRUD 작업 외에 메시지 그룹 관련 특수 메서드를 정의합니다.
    /// </summary>
    public interface IMessageGroupRepository : IRepository<MessageGroup>
    {
        /// <summary>
        /// 활성화된 메시지 그룹만 조회합니다.
        /// </summary>
        /// <returns>활성화된 메시지 그룹 목록</returns>
        Task<IEnumerable<MessageGroup>> GetActiveGroupsAsync();

        /// <summary>
        /// 메시지 그룹에 회원을 추가합니다.
        /// </summary>
        /// <param name="groupId">회원을 추가할 그룹 ID</param>
        /// <param name="memberId">추가할 회원 ID</param>
        Task AddMemberToGroupAsync(int groupId, int memberId);

        /// <summary>
        /// 메시지 그룹에서 회원을 제거합니다.
        /// </summary>
        /// <param name="groupId">회원을 제거할 그룹 ID</param>
        /// <param name="memberId">제거할 회원 ID</param>
        Task RemoveMemberFromGroupAsync(int groupId, int memberId);
    }
}
