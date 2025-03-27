// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 메시지 그룹과 회원의 다대다 관계를 위한 연결 테이블 모델
// 설명: 이 클래스는 메시지 그룹과 회원 간의 다대다 관계를 표현합니다.
namespace SMSAlarmSystem.Core.Models
{
    public class MessageGroupMember
    {
        public int MessageGroupId { get; set; } // 메시지 그룹 ID
        public int MemberId { get; set; } // 회원 ID

        // 탐색 속성
        public MessageGroup MessageGroup { get; set; } = null!; // 메시지 그룹 참조
        public Member Member { get; set; } = null!; // 회원 참조
    }
}
