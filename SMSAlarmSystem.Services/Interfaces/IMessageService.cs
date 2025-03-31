using SMSAlarmSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 메시지 관련 비즈니스 로직을 정의하는 인터페이스
// 설명: 메시지 CRUD 및 관련 기능에 대한 계약을 정의합니다.
namespace SMSAlarmSystem.Services.Interfaces
{
    /// <summary>
    /// 메시지 관련 비즈니스 로직을 정의하는 인터페이스
    /// 메시지 조회, 추가, 수정, 삭제 및 특정 조건에 따른 메시지 검색 기능을 제공합니다.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// 모든 메시지를 조회합니다.
        /// </summary>
        /// <returns>메시지 목록</returns>
        Task<IEnumerable<Message>> GetAllMessagesAsync();

        /// <summary>
        /// ID로 특정 메시지를 조회합니다.
        /// </summary>
        /// <param name="id">조회할 메시지 ID</param>
        /// <returns>조회된 메시지 또는 null(메시지가 없는 경우)</returns>
        Task<Message?> GetMessageByIdAsync(int id);

        /// <summary>
        /// 지정된 날짜 범위 내의 메시지를 조회합니다.
        /// </summary>
        /// <param name="start">시작 날짜</param>
        /// <param name="end">종료 날짜</param>
        /// <returns>날짜 범위 내 메시지 목록</returns>
        Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(DateTime start, DateTime end);

        /// <summary>
        /// 특정 그룹에 속한 메시지를 조회합니다.
        /// </summary>
        /// <param name="groupId">그룹 ID</param>
        /// <returns>해당 그룹의 메시지 목록</returns>
        Task<IEnumerable<Message>> GetMessagesByGroupIdAsync(int groupId);

        /// <summary>
        /// 특정 알람 포인트와 관련된 메시지를 조회합니다.
        /// </summary>
        /// <param name="alarmPointId">알람 포인트 ID</param>
        /// <returns>해당 알람 포인트의 메시지 목록</returns>
        Task<IEnumerable<Message>> GetMessagesByAlarmPointIdAsync(int alarmPointId);

        /// <summary>
        /// 새 메시지를 추가합니다.
        /// </summary>
        /// <param name="message">추가할 메시지 객체</param>
        /// <returns>추가 성공 여부</returns>
        Task<bool> AddMessageAsync(Message message);

        /// <summary>
        /// 기존 메시지를 업데이트합니다.
        /// </summary>
        /// <param name="message">업데이트할 메시지 객체</param>
        /// <returns>업데이트 성공 여부</returns>
        Task<bool> UpdateMessageAsync(Message message);

        /// <summary>
        /// 메시지를 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 메시지 ID</param>
        /// <returns>삭제 성공 여부</returns>
        Task<bool> DeleteMessageAsync(int id);

        /// <summary>
        /// 메시지의 상태를 업데이트합니다.
        /// </summary>
        /// <param name="id">업데이트할 메시지 ID</param>
        /// <param name="status">새 상태 값</param>
        /// <returns>상태 업데이트 성공 여부</returns>
        Task<bool> UpdateMessageStatusAsync(int id, string status);
    }
}