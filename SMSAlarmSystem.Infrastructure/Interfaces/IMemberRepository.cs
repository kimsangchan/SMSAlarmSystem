// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 회원 데이터 접근을 위한 인터페이스
// 설명: 회원 관련 데이터 접근 메서드를 정의합니다.

using SMSAlarmSystem.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Core.Interfaces
{
    /// <summary>
    /// 회원 데이터 접근을 위한 인터페이스
    /// 기본 CRUD 작업 외에 회원 관련 특수 조회 메서드를 정의합니다.
    /// </summary>
    public interface IMemberRepository : IRepository<Member>
    {
        /// <summary>
        /// 전화번호로 회원을 조회합니다.
        /// </summary>
        /// <param name="phoneNumber">조회할 회원의 전화번호</param>
        /// <returns>조회된 회원 또는 null(회원이 없는 경우)</returns>
        Task<Member?> GetByPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// 특정 그룹에 속한 회원 목록을 조회합니다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <returns>해당 그룹에 속한 회원 목록</returns>
        Task<IEnumerable<Member>> GetMembersByGroupIdAsync(int groupId);
    }
}
