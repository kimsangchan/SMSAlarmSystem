// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: SMS 발송 서비스 구현 클래스
// 설명: 외부 SMS API와 통신하여 단일 또는 다수의 수신자에게 SMS 메시지를 발송하는 서비스입니다.
//       테스트 모드를 지원하여 실제 API 호출 없이 테스트가 가능합니다.

using Microsoft.Extensions.Logging;
using SMSAlarmSystem.Core.Interfaces;
using SMSAlarmSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Services.Services
{
    /// <summary>
    /// SMS 메시지 발송을 담당하는 서비스 클래스
    /// ISMSService 인터페이스를 구현하여 단일 및 대량 SMS 발송 기능을 제공합니다.
    /// </summary>
    public class SMSService : ISMSService
    {
        // HTTP 통신을 위한 클라이언트
        private readonly HttpClient _httpClient;

        // SMS API 인증을 위한 API 키
        private readonly string _apiKey;

        // SMS API 엔드포인트 URL
        private readonly string _apiUrl;

        // 로깅을 위한 로거 인스턴스
        private readonly ILogger<SMSService> _logger;

        // 테스트 모드 활성화 여부 (true: API 호출 없이 테스트, false: 실제 API 호출)
        private readonly bool _useTestMode;

        /// <summary>
        /// SMS 서비스 생성자
        /// </summary>
        /// <param name="httpClient">HTTP 요청을 보내기 위한 HttpClient 인스턴스</param>
        /// <param name="apiKey">SMS API 인증을 위한 API 키</param>
        /// <param name="apiUrl">SMS API 엔드포인트 URL</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <param name="useTestMode">테스트 모드 활성화 여부 (기본값: true)</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public SMSService(HttpClient httpClient, string apiKey, string apiUrl, ILogger<SMSService> logger, bool useTestMode = true)
        {
            // null 체크 및 예외 처리
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient), "HttpClient는 null이 될 수 없습니다.");
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey), "API 키는 null이 될 수 없습니다.");
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl), "API URL은 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger는 null이 될 수 없습니다.");
            _useTestMode = useTestMode;

            // 서비스 초기화 정보 로깅
            _logger.LogInformation("SMS 서비스 초기화 완료: API URL={ApiUrl}, 테스트 모드={TestMode}", _apiUrl, _useTestMode);
        }

        /// <summary>
        /// 단일 수신자에게 SMS 메시지를 발송합니다.
        /// </summary>
        /// <param name="phoneNumber">수신자 전화번호 (E.164 형식 권장: +821012345678)</param>
        /// <param name="message">발송할 메시지 내용</param>
        /// <returns>발송 성공 여부 (true: 성공, false: 실패)</returns>
        public async Task<bool> SendSMSAsync(string phoneNumber, string message)
        {
            // 입력값 유효성 검사
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
                // 테스트 모드인 경우 실제 API 호출 없이 처리
                if (_useTestMode)
                {
                    _logger.LogInformation("[테스트 모드] SMS 발송: 수신자={PhoneNumber}, 내용={Message}", phoneNumber, message);

                    // 테스트 목적으로 90%의 확률로 성공, 10%의 확률로 실패 반환
                    var isSuccess = new Random().Next(100) < 90;

                    if (isSuccess)
                    {
                        _logger.LogInformation("[테스트 모드] SMS 발송 성공: 수신자={PhoneNumber}", phoneNumber);
                    }
                    else
                    {
                        _logger.LogWarning("[테스트 모드] SMS 발송 실패: 수신자={PhoneNumber}", phoneNumber);
                    }

                    return isSuccess;
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
                _logger.LogDebug("SMS API 호출 시작: 수신자={PhoneNumber}, 메시지 길이={MessageLength}",
                    phoneNumber, message.Length);

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
            catch (HttpRequestException ex)
            {
                // HTTP 요청 관련 예외 처리
                _logger.LogError(ex, "SMS 발송 중 HTTP 요청 예외 발생: 수신자={PhoneNumber}, 메시지={Message}",
                    phoneNumber, ex.Message);
                return false;
            }
            catch (JsonException ex)
            {
                // JSON 직렬화 관련 예외 처리
                _logger.LogError(ex, "SMS 발송 중 JSON 직렬화 예외 발생: 수신자={PhoneNumber}, 메시지={Message}",
                    phoneNumber, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                // 기타 예외 처리
                _logger.LogError(ex, "SMS 발송 중 예외 발생: 수신자={PhoneNumber}, 예외 유형={ExceptionType}, 메시지={Message}",
                    phoneNumber, ex.GetType().Name, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 다수의 수신자에게 동일한 SMS 메시지를 발송합니다.
        /// </summary>
        /// <param name="phoneNumbers">수신자 전화번호 목록</param>
        /// <param name="message">발송할 메시지 내용</param>
        /// <returns>각 전화번호별 발송 결과를 담은 Dictionary (key: 전화번호, value: 발송 성공 여부)</returns>
        public async Task<Dictionary<string, bool>> SendBulkSMSAsync(IEnumerable<string> phoneNumbers, string message)
        {
            // 입력값 유효성 검사
            if (phoneNumbers == null)
            {
                _logger.LogError("대량 SMS 발송 실패: 전화번호 목록이 null입니다.");
                return new Dictionary<string, bool>();
            }

            // 빈 목록 체크
            var phoneNumberList = phoneNumbers.ToList();
            if (phoneNumberList.Count == 0)
            {
                _logger.LogWarning("대량 SMS 발송: 전화번호 목록이 비어 있습니다.");
                return new Dictionary<string, bool>();
            }

            if (string.IsNullOrEmpty(message))
            {
                _logger.LogError("대량 SMS 발송 실패: 메시지가 null 또는 빈 문자열입니다.");
                return phoneNumberList.ToDictionary(p => p, p => false);
            }

            _logger.LogInformation("대량 SMS 발송 시작: 수신자 수={Count}", phoneNumberList.Count);

            var results = new Dictionary<string, bool>();

            // 각 전화번호에 대해 개별적으로 SMS 발송
            foreach (var phoneNumber in phoneNumberList)
            {
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    _logger.LogWarning("대량 SMS 발송 중 건너뜀: 전화번호가 null 또는 빈 문자열입니다.");
                    continue;
                }

                // 개별 SMS 발송 및 결과 저장
                results[phoneNumber] = await SendSMSAsync(phoneNumber, message);
            }

            // 발송 결과 통계 로깅
            var successCount = results.Count(r => r.Value);
            var failCount = results.Count - successCount;
            _logger.LogInformation("대량 SMS 발송 완료: 성공={SuccessCount}, 실패={FailCount}", successCount, failCount);

            return results;
        }
    }
}
