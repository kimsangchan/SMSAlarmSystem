// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 발송된 메시지 정보를 저장하는 모델 클래스
// 설명: 이 클래스는 발송된 메시지의 기본 정보를 포함하며, 데이터베이스 테이블과 매핑됩니다.

using System;

namespace SMSAlarmSystem.Core.Models
{
    /// <summary>
    /// 발송된 SMS 메시지 정보를 저장하는 모델 클래스
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 메시지의 고유 식별자
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 메시지 내용
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 메시지 발송 시간
        /// </summary>
        public DateTime SendTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 메시지 생성 시간
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 메시지 업데이트 시간
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 발생한 알람 포인트의 ID
        /// </summary>
        public int AlarmPointId { get; set; }

        /// <summary>
        /// 메시지가 발송된 그룹의 ID
        /// </summary>
        public int MessageGroupId { get; set; }

        /// <summary>
        /// 메시지 발송 상태
        /// 가능한 값: "발송 대기", "발송 중", "발송 완료", "발송 실패"
        /// </summary>
        public string Status { get; set; } = "발송 대기";

        /// <summary>
        /// 수신자 전화번호 (E.164 형식 권장: +821012345678)
        /// </summary>
        public string RecipientNumber { get; set; } = string.Empty;

        /// <summary>
        /// 메시지 발송 시도 횟수
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// 마지막 발송 시도 시간
        /// </summary>
        public DateTime? LastAttemptTime { get; set; }

        /// <summary>
        /// 오류 메시지 (발송 실패 시)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 외부 시스템에서 할당한 메시지 ID (SMS 제공업체에서 반환한 ID)
        /// </summary>
        public string? ExternalMessageId { get; set; }

        // 탐색 속성 (Entity Framework 관계 매핑용)
        /// <summary>
        /// 알람 포인트 참조 - 이 메시지를 발생시킨 알람 포인트
        /// </summary>
        public AlarmPoint? AlarmPoint { get; set; }

        /// <summary>
        /// 메시지 그룹 참조 - 이 메시지가 발송된 그룹
        /// </summary>
        public MessageGroup? MessageGroup { get; set; }
    }
}
