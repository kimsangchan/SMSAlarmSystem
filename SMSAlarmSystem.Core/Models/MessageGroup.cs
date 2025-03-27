// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 메시지 그룹 정보를 저장하는 모델 클래스
// 설명: 이 클래스는 메시지 그룹의 기본 정보를 포함하며, 데이터베이스 테이블과 매핑됩니다.
using System;
using System.Collections.Generic;

namespace SMSAlarmSystem.Core.Models
{
    public class MessageGroup
    {
        public int Id { get; set; } // 그룹 ID
        public string Name { get; set; } = string.Empty; // 그룹 이름
        public string? Description { get; set; } // 그룹 설명 (null 허용)
        public bool IsActive { get; set; } = true; // 활성 상태

        // 회원과의 관계 (다대다 관계)
        public ICollection<MessageGroupMember> GroupMembers { get; set; } = new List<MessageGroupMember>();
        // 알람 포인트와의 관계 (일대다 관계)
        public ICollection<AlarmPoint> AlarmPoints { get; set; } = new List<AlarmPoint>();
    }
}
