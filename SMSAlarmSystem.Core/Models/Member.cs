// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 회원 정보를 저장하는 모델 클래스
// 설명: 이 클래스는 회원의 기본 정보를 포함하며, 데이터베이스 테이블과 매핑됩니다.
using System;
using System.Collections.Generic;

namespace SMSAlarmSystem.Core.Models
{
    public class Member
    {
        public int Id { get; set; } // 회원 ID
        public string Name { get; set; } = string.Empty; // 회원 이름
        public string PhoneNumber { get; set; } = string.Empty; // 전화번호
        public string? Email { get; set; } // 이메일 (null 허용)
        public DateTime RegisterDate { get; set; } = DateTime.Now; // 등록일
        public bool IsActive { get; set; } = true; // 활성 상태

        // 그룹과의 관계 (다대다 관계)
        public ICollection<MessageGroupMember> MessageGroupMembers { get; set; } = new List<MessageGroupMember>();
    }
}
