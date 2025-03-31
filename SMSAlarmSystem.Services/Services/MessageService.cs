// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: 메시지 관련 비즈니스 로직을 처리하는 서비스
// 설명: 메시지 CRUD 및 관련 기능을 제공합니다.
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Services.Services
{
    public class MessageService  :IMessageService

    {
        // 메시지 데이터 접근을 위한 리포지토리
        private readonly IMessageRepository _messageRepository;

        // 로깅을 위한 로거 인스턴스
        private readonly ILogger<MessageService> _logger;

        /// <summary>
        /// MessageService 생성자
        /// </summary>
        /// <param name="messageRepository">메시지 리포지토리 인스턴스</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public MessageService(IMessageRepository messageRepository, ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 모든 메시지를 조회합니다.
        /// </summary>
        /// <returns>메시지 목록 또는 오류 발생 시 빈 목록</returns>
        public async Task<IEnumerable<Message>> GetAllMessagesAsync()
        {
            try
            {
                _logger.LogInformation("모든 메시지 조회 시작");
                return await _messageRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "모든 메시지 조회 중 오류 발생");
                return new List<Message>();
            }
        }
        /// <summary>
        /// ID로 특정 메시지를 조회합니다.
        /// </summary>
        /// <param name="id">조회할 메시지 ID</param>
        /// <returns>조회된 메시지 또는 null(메시지가 없거나 오류 발생 시)</returns>
        public async Task<Message?> GetMessageByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("메시지 조회 시작: ID={Id}", id);
                return await _messageRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 조회 중 오류 발생: ID={Id}", id);
                return null;
            }
        }

        /// <summary>
        /// 지정된 날짜 범위 내의 메시지를 조회합니다.
        /// </summary>
        /// <param name="start">시작 날짜</param>
        /// <param name="end">종료 날짜</param>
        /// <returns>날짜 범위 내 메시지 목록 또는 오류 발생 시 빈 목록</returns>
        public async Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(DateTime start, DateTime end)
        {
            // 날짜 범위 유효성 검사
            if (start > end)
            {
                _logger.LogWarning("날짜 범위로 메시지 조회 실패: 시작 날짜가 종료 날짜보다 늦습니다. Start={Start}, End={End}", start, end);
                return new List<Message>();
            }

            try
            {
                _logger.LogInformation("날짜 범위로 메시지 조회 시작: Start={Start}, End={End}", start, end);
                var messages = await _messageRepository.GetMessagesByDateRangeAsync(start, end);

                _logger.LogInformation("날짜 범위로 메시지 조회 완료: Start={Start}, End={End}, 조회된 메시지 수={Count}",
                    start, end, messages is ICollection<Message> collection ? collection.Count : -1);

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "날짜 범위로 메시지 조회 중 오류 발생: Start={Start}, End={End}, 오류={ErrorMessage}",
                    start, end, ex.Message);
                return new List<Message>();
            }
        }
        /// <summary>
        /// 특정 그룹에 속한 메시지를 조회합니다.
        /// </summary>
        /// <param name="groupId">그룹 ID</param>
        /// <returns>해당 그룹의 메시지 목록 또는 오류 발생 시 빈 목록</returns>
        public async Task<IEnumerable<Message>> GetMessagesByGroupIdAsync(int groupId)
        {
            if (groupId <= 0)
            {
                _logger.LogWarning("그룹 ID로 메시지 조회 실패: 유효하지 않은 그룹 ID={GroupId}", groupId);
                return new List<Message>();
            }

            try
            {
                _logger.LogInformation("그룹 ID로 메시지 조회 시작: GroupID={GroupId}", groupId);
                var messages = await _messageRepository.GetMessagesByGroupIdAsync(groupId);

                _logger.LogInformation("그룹 ID로 메시지 조회 완료: GroupID={GroupId}, 조회된 메시지 수={Count}",
                    groupId, messages is ICollection<Message> collection ? collection.Count : -1);

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹 ID로 메시지 조회 중 오류 발생: GroupID={GroupId}, 오류={ErrorMessage}",
                    groupId, ex.Message);
                return new List<Message>();
            }
        }

        /// <summary>
        /// 특정 알람 포인트와 관련된 메시지를 조회합니다.
        /// </summary>
        /// <param name="alarmPointId">알람 포인트 ID</param>
        /// <returns>해당 알람 포인트의 메시지 목록 또는 오류 발생 시 빈 목록</returns>
        public async Task<IEnumerable<Message>> GetMessagesByAlarmPointIdAsync(int alarmPointId)
        {
            if (alarmPointId <= 0)
            {
                _logger.LogWarning("알람 포인트 ID로 메시지 조회 실패: 유효하지 않은 알람 포인트 ID={AlarmPointId}", alarmPointId);
                return new List<Message>();
            }

            try
            {
                _logger.LogInformation("알람 포인트 ID로 메시지 조회 시작: AlarmPointID={AlarmPointId}", alarmPointId);
                var messages = await _messageRepository.GetMessagesByAlarmPointIdAsync(alarmPointId);

                _logger.LogInformation("알람 포인트 ID로 메시지 조회 완료: AlarmPointID={AlarmPointId}, 조회된 메시지 수={Count}",
                    alarmPointId, messages is ICollection<Message> collection ? collection.Count : -1);

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트 ID로 메시지 조회 중 오류 발생: AlarmPointID={AlarmPointId}, 오류={ErrorMessage}",
                    alarmPointId, ex.Message);
                return new List<Message>();
            }
        }

        /// <summary>
        /// 새 메시지를 추가합니다.
        /// </summary>
        /// <param name="message">추가할 메시지 객체</param>
        /// <returns>추가 성공 여부</returns>
        public async Task<bool> AddMessageAsync(Message message)
        {
            if (message == null)
            {
                _logger.LogError("메시지 추가 실패: 메시지 객체가 null입니다.");
                return false;
            }

            // 메시지 내용 유효성 검사
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                _logger.LogError("메시지 추가 실패: 메시지 내용이 비어 있습니다.");
                return false;
            }

            try
            {
                _logger.LogInformation("메시지 추가 시작: Content={Content}", message.Content);

                // 생성 시간이 설정되지 않은 경우 현재 시간으로 설정
                if (message.CreatedAt == default)
                {
                    message.CreatedAt = DateTime.UtcNow;
                }

                await _messageRepository.AddAsync(message);
                _logger.LogInformation("메시지 추가 성공: ID={Id}, Content={Content}", message.Id, message.Content);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 추가 중 오류 발생: Content={Content}, 오류={ErrorMessage}",
                    message.Content, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 기존 메시지를 업데이트합니다.
        /// </summary>
        /// <param name="message">업데이트할 메시지 객체</param>
        /// <returns>업데이트 성공 여부</returns>
        public async Task<bool> UpdateMessageAsync(Message message)
        {
            if (message == null)
            {
                _logger.LogError("메시지 업데이트 실패: 메시지 객체가 null입니다.");
                return false;
            }

            if (message.Id <= 0)
            {
                _logger.LogError("메시지 업데이트 실패: 유효하지 않은 ID={Id}", message.Id);
                return false;
            }

            try
            {
                // 메시지가 존재하는지 확인
                var existingMessage = await _messageRepository.GetByIdAsync(message.Id);
                if (existingMessage == null)
                {
                    _logger.LogWarning("메시지 업데이트 실패: 메시지를 찾을 수 없음, ID={Id}", message.Id);
                    return false;
                }

                _logger.LogInformation("메시지 업데이트 시작: ID={Id}, Status={Status}", message.Id, message.Status);

                // 업데이트 시간 설정
                message.UpdatedAt = DateTime.UtcNow;

                await _messageRepository.UpdateAsync(message);
                _logger.LogInformation("메시지 업데이트 성공: ID={Id}, Status={Status}", message.Id, message.Status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 업데이트 중 오류 발생: ID={Id}, Status={Status}, 오류={ErrorMessage}",
                    message.Id, message.Status, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 메시지를 삭제합니다.
        /// </summary>
        /// <param name="id">삭제할 메시지 ID</param>
        /// <returns>삭제 성공 여부</returns>
        public async Task<bool> DeleteMessageAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogError("메시지 삭제 실패: 유효하지 않은 ID={Id}", id);
                return false;
            }

            try
            {
                _logger.LogInformation("메시지 삭제 시작: ID={Id}", id);

                // 메시지가 존재하는지 확인
                var message = await _messageRepository.GetByIdAsync(id);
                if (message == null)
                {
                    _logger.LogWarning("메시지 삭제 실패: 메시지를 찾을 수 없음, ID={Id}", id);
                    return false;
                }

                await _messageRepository.DeleteAsync(message);
                _logger.LogInformation("메시지 삭제 성공: ID={Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 삭제 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 메시지의 상태를 업데이트합니다.
        /// </summary>
        /// <param name="id">업데이트할 메시지 ID</param>
        /// <param name="status">새 상태 값</param>
        /// <returns>상태 업데이트 성공 여부</returns>
        public async Task<bool> UpdateMessageStatusAsync(int id, string status)
        {
            if (id <= 0)
            {
                _logger.LogError("메시지 상태 업데이트 실패: 유효하지 않은 ID={Id}", id);
                return false;
            }

            if (string.IsNullOrEmpty(status))
            {
                _logger.LogError("메시지 상태 업데이트 실패: 상태가 null 또는 빈 문자열입니다.");
                return false;
            }

            try
            {
                _logger.LogInformation("메시지 상태 업데이트 시작: ID={Id}, Status={Status}", id, status);

                // 메시지가 존재하는지 확인
                var message = await _messageRepository.GetByIdAsync(id);
                if (message == null)
                {
                    _logger.LogWarning("메시지 상태 업데이트 실패: 메시지를 찾을 수 없음, ID={Id}", id);
                    return false;
                }

                // 상태가 이미 동일한지 확인 (불필요한 업데이트 방지)
                if (message.Status == status)
                {
                    _logger.LogInformation("메시지 상태 업데이트 건너뜀: 상태가 이미 '{Status}'임, ID={Id}", status, id);
                    return true;
                }

                message.Status = status;
                message.UpdatedAt = DateTime.UtcNow;

                await _messageRepository.UpdateAsync(message);
                _logger.LogInformation("메시지 상태 업데이트 성공: ID={Id}, Status={Status}", id, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 상태 업데이트 중 오류 발생: ID={Id}, Status={Status}, 오류={ErrorMessage}",
                    id, status, ex.Message);
                return false;
            }
        }
    }
}