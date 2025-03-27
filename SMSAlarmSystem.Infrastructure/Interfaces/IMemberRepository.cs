// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 회원 저장소 인터페이스
// 설명: 이 인터페이스는 회원 관련 데이터 액세스를 위한 메서드를 정의합니다.
using SMSAlarmSystem.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Core.Interfaces
{
    public interface IMemberRepository : IRepository<Member>
    {
        // 전화번호로 회원 찾기
        Task<Member?> GetByPhoneNumberAsync(string phoneNumber);

        // 특정 그룹에 속한 회원 목록 가져오기
        Task<IEnumerable<Member>> GetMembersByGroupIdAsync(int groupId);
    }
}
