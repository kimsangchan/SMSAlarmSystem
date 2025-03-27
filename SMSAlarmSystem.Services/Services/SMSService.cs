// 작성자: Sangchan, Kim
// 작성일: 2025-03-27
// 기능: SMS 발송 서비스 구현 클래스
// 설명: 외부 SMS API와 통신하여 메시지를 발송하는 서비스입니다.
using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Infrastructure.Services
{
    public class SMSService : ISMSService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly ILogger<SMSService> _logger;
        private readonly bool _useTestMode;

        // 생성자 - HttpClient, 설정 및 로거 주입
        public SMSService(HttpClient httpClient, string apiKey, string apiUrl, ILogger<SMSService> logger, bool useTestMode = true)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _useTestMode = useTestMode;

            _logger.LogInformation("SMS 서비스 초기화 완료: API URL={ApiUrl}, 테스트 모드={TestMode}", _apiUrl, _useTestMode);
        }

        // 단일 메시지 발송
        public async Task<bool> SendSMSAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                _logger.LogError("SMS 발송 실패: 전화번호가 null 또는 빈 문자열입니다.");
                return false;
            }

            if (string.IsNullOrEmpty(message))
            {
                _logger.LogError("SMS 발송 실패: 메시지가 null 또는 빈 문자열입니다.");
                return false;
            }

            try
            {
                if (_useTestMode)
                {
                    // 테스트 모드에서는 실제 API 호출 없이 성공 응답 반환
                    _logger.LogInformation("[테스트 모드] SMS 발송: 수신자={PhoneNumber}, 내용={Message}", phoneNumber, message);

                    // 테스트 목적으로 90%의 확률로 성공, 10%의 확률로 실패 반환
                    return new Random().Next(100) < 90;
                }

                // SMS API 요청 데이터 준비
                var requestData = new
                {
                    apiKey = _apiKey,
                    to = phoneNumber,
                    message = message,
                    from = "SMSAlarm" // 발신자 번호 또는 이름
                };

                // JSON으로 변환
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                // API 호출 전 로깅
                _logger.LogDebug("SMS API 호출 시작: 수신자={PhoneNumber}", phoneNumber);

                // API 호출
                var response = await _httpClient.PostAsync(_apiUrl, content);

                // 응답 확인 및 로깅
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SMS 발송 성공: 수신자={PhoneNumber}", phoneNumber);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("SMS 발송 실패: 수신자={PhoneNumber}, 상태 코드={StatusCode}, 응답={Response}",
                        phoneNumber, (int)response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                // 오류 발생 시 로깅
                _logger.LogError(ex, "SMS 발송 중 예외 발생: 수신자={PhoneNumber}, 메시지={Message}", phoneNumber, ex.Message);
                return false;
            }
        }

        // 다수 수신자에게 메시지 발송
        public async Task<Dictionary<string, bool>> SendBulkSMSAsync(IEnumerable<string> phoneNumbers, string message)
        {
            if (phoneNumbers == null)
            {
                _logger.LogError("대량 SMS 발송 실패: 전화번호 목록이 null입니다.");
                return new Dictionary<string, bool>();
            }

            if (string.IsNullOrEmpty(message))
            {
                _logger.LogError("대량 SMS 발송 실패: 메시지가 null 또는 빈 문자열입니다.");
                return phoneNumbers.ToDictionary(p => p, p => false);
            }

            _logger.LogInformation("대량 SMS 발송 시작: 수신자 수={Count}", phoneNumbers.Count());

            var results = new Dictionary<string, bool>();
            foreach (var phoneNumber in phoneNumbers)
            {
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    _logger.LogWarning("대량 SMS 발송 중 건너뜀: 전화번호가 null 또는 빈 문자열입니다.");
                    continue;
                }

                results[phoneNumber] = await SendSMSAsync(phoneNumber, message);
            }

            var successCount = results.Count(r => r.Value);
            var failCount = results.Count - successCount;
            _logger.LogInformation("대량 SMS 발송 완료: 성공={SuccessCount}, 실패={FailCount}", successCount, failCount);

            return results;
        }
    }
}
