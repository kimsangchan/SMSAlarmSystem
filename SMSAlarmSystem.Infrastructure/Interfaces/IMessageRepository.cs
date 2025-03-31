// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 메시지 데이터 접근을 위한 인터페이스
// 설명: 메시지 관련 데이터 접근 메서드를 정의합니다.

using SMSAlarmSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Core.Interfaces
{
    /// <summary>
    /// 메시지 데이터 접근을 위한 인터페이스
    /// 기본 CRUD 작업 외에 메시지 관련 특수 조회 메서드를 정의합니다.
    /// </summary>
    public interface IMessageRepository : IRepository<Message>
    {
        /// <summary>
        /// 특정 기간 내에 발송된 메시지를 조회합니다.
        /// </summary>
        /// <param name="start">조회 시작 날짜</param>
        /// <param name="end">조회 종료 날짜</param>
        /// <returns>해당 기간 내 발송된 메시지 목록</returns>
        Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(DateTime start, DateTime end);

        /// <summary>
        /// 특정 그룹에 발송된 메시지를 조회합니다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <returns>해당 그룹에 발송된 메시지 목록</returns>
        Task<IEnumerable<Message>> GetMessagesByGroupIdAsync(int groupId);

        /// <summary>
        /// 특정 알람 포인트에서 발생한 메시지를 조회합니다.
        /// </summary>
        /// <param name="alarmPointId">조회할 알람 포인트 ID</param>
        /// <returns>해당 알람 포인트에서 발생한 메시지 목록</returns>
        Task<IEnumerable<Message>> GetMessagesByAlarmPointIdAsync(int alarmPointId);
    }
}
