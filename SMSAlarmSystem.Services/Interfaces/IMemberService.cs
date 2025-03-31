// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 회원 관련 비즈니스 로직을 정의하는 인터페이스
// 설명: 회원 CRUD 및 관련 기능에 대한 계약을 정의합니다.

using SMSAlarmSystem.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Services.Interfaces
{
    /// <summary>
    /// 회원 관련 비즈니스 로직을 정의하는 인터페이스
    /// </summary>
    public interface IMemberService
    {
        /// <summary>
        /// 모든 회원을 조회합니다.
        /// </summary>
        /// <returns>회원 목록</returns>
        Task<IEnumerable<Member>> GetAllMembersAsync();

        /// <summary>
        /// ID로 특정 회원을 조회합니다.
        /// </summary>
        /// <param name="id">조회할 회원 ID</param>
        /// <returns>조회된 회원 또는 null(회원이 없는 경우)</returns>
        Task<Member?> GetMemberByIdAsync(int id);

        /// <summary>
        /// 전화번호로 회원을 조회합니다.
        /// </summary>
        /// <param name="phoneNumber">조회할 회원의 전화번호</param>
        /// <returns>조회된 회원 또는 null(회원이 없는 경우)</returns>
        Task<Member?> GetMemberByPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// 새 회원을 추가합니다.
        /// </summary>
        /// <param name="member">추가할 회원 객체</param>
        /// <returns>추가 성공 여부</returns>
        Task<bool> AddMemberAsync(Member member);

        /// <summary>
        /// 기존 회원 정보를 업데이트합니다.
        /// </summary>
        /// <param name="member">업데이트할 회원 객체</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> UpdateMemberAsync(Member member);

        /// <summary>
        /// 회원을 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 회원 ID</param>
        /// <returns>삭제 성공 여부</returns>
        Task<bool> DeleteMemberAsync(int id);
    }
}
