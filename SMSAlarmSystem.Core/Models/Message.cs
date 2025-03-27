// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 발송된 메시지 정보를 저장하는 모델 클래스
// 설명: 이 클래스는 발송된 메시지의 기본 정보를 포함하며, 데이터베이스 테이블과 매핑됩니다.
using System;

namespace SMSAlarmSystem.Core.Models
{
    public class Message
    {
        public int Id { get; set; } // 메시지 ID
        public string Content { get; set; } = string.Empty; // 메시지 내용
        public DateTime SendTime { get; set; } = DateTime.Now; // 발송 시간
        public int AlarmPointId { get; set; } // 발생한 알람 포인트 ID
        public int MessageGroupId { get; set; } // 발송된 그룹 ID
        public string Status { get; set; } = "발송 대기"; // 상태 (발송 대기, 발송 중, 발송 완료, 발송 실패)

        // 탐색 속성
        public AlarmPoint? AlarmPoint { get; set; } // 알람 포인트 참조
        public MessageGroup? MessageGroup { get; set; } // 메시지 그룹 참조
    }
}
