// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 오류 로그를 저장하는 모델 클래스
// 설명: 이 클래스는 애플리케이션에서 발생한 오류 정보를 포함하며, 데이터베이스 테이블과 매핑됩니다.
using System;

namespace SMSAlarmSystem.Core.Models
{
    public class ErrorLog
    {
        public int Id { get; set; } // 로그 ID
        public string Message { get; set; } = string.Empty; // 오류 메시지
        public string? StackTrace { get; set; } // 스택 트레이스 (null 허용)
        public string? Source { get; set; } // 오류 소스 (null 허용)
        public DateTime LogTime { get; set; } = DateTime.Now; // 로그 시간
        public string LogLevel { get; set; } = "Error"; // 로그 레벨 (Error, Warning, Info 등)
    }
}
