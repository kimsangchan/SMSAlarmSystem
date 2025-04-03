// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 알람 포인트 정보를 저장하는 모델 클래스
// 설명: 이 클래스는 알람 포인트의 기본 정보를 포함하며, 데이터베이스 테이블과 매핑됩니다.
using System;

namespace SMSAlarmSystem.Core.Models
{
    public class AlarmPoint
    {
        public int Id { get; set; } // 알람 포인트 ID
        public string Name { get; set; } = string.Empty; // 알람 포인트 이름
        public string? Description { get; set; } // 설명 (null 허용)
        public string? Condition { get; set; } // 알람 조건 (null 허용)
        public int MessageGroupId { get; set; } // 연결된 메시지 그룹 ID
        public bool IsActive { get; set; } = true; // 활성 상태
        public string? ExternalId { get; set; } // 외부 시스템 ID (null 허용)
        public DateTime CreatedAt { get; set; } // 생성 시간
        public DateTime UpdatedAt { get; set; } // 수정 시간

        // 탐색 속성
        public MessageGroup? MessageGroup { get; set; } // 메시지 그룹 참조
    }
}
