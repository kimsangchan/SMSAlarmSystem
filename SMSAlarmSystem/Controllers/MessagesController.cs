// 작성자: Sangchan, Kim
// 작성일: 2025-03-31
// 기능: 메시지 관리 컨트롤러
// 설명: 메시지 조회, 상세 보기 및 필터링 기능을 제공하는 MVC 컨트롤러입니다.

using Microsoft.AspNetCore.Mvc;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Models;
using SMSAlarmSystem.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Controllers
{
    /// <summary>
    /// 메시지 관리 기능을 제공하는 MVC 컨트롤러
    /// 메시지 목록 조회, 상세 보기, 그룹별/알람 포인트별 필터링 기능을 제공합니다.
    /// </summary>
    public class MessagesController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;

        /// <summary>
        /// MessagesController 생성자
        /// </summary>
        /// <param name="messageService">메시지 서비스 인스턴스</param>
        /// <param name="logger">로깅을 위한 ILogger 인스턴스</param>
        /// <exception cref="ArgumentNullException">필수 매개변수가 null인 경우 발생</exception>
        public MessagesController(IMessageService messageService, ILogger<MessagesController> logger)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService), "메시지 서비스는 null이 될 수 없습니다.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "로거는 null이 될 수 없습니다.");

            _logger.LogInformation("MessagesController 초기화 완료");
        }

        /// <summary>
        /// 메시지 목록 페이지를 표시합니다.
        /// </summary>
        /// <param name="page">페이지 번호 (1부터 시작)</param>
        /// <param name="pageSize">페이지당 표시할 메시지 수</param>
        /// <param name="startDate">조회 시작 날짜</param>
        /// <param name="endDate">조회 종료 날짜</param>
        /// <returns>페이지네이션이 적용된 메시지 목록 뷰</returns>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // 페이지 번호와 페이지 크기 유효성 검사
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // 최대 100개로 제한

                // 날짜 범위 설정 (기본값: 최근 7일)
                var today = DateTime.Today;
                var defaultStartDate = today.AddDays(-7);
                var defaultEndDate = today;

                var actualStartDate = startDate ?? defaultStartDate;
                var actualEndDate = endDate ?? defaultEndDate;

                // 종료일이 시작일보다 이전인 경우 조정
                if (actualEndDate < actualStartDate)
                {
                    actualEndDate = actualStartDate;
                }

                _logger.LogInformation("메시지 목록 조회 시작: 페이지={Page}, 페이지크기={PageSize}, 시작일={StartDate}, 종료일={EndDate}",
                    page, pageSize, actualStartDate, actualEndDate);

                // 날짜 범위로 메시지 조회
                var messages = await _messageService.GetMessagesByDateRangeAsync(actualStartDate, actualEndDate);

                // null 체크 (방어적 프로그래밍)
                if (messages == null)
                {
                    _logger.LogWarning("메시지 서비스에서 null 반환됨. 빈 목록으로 대체합니다.");
                    messages = new List<Message>();
                }

                // 최신 메시지부터 표시하도록 정렬
                var orderedMessages = messages.OrderByDescending(m => m.SendTime);

                // 페이지네이션 적용
                var totalItems = orderedMessages.Count();
                var pagedMessages = orderedMessages
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // 페이지네이션 뷰 모델 생성
                var model = new PaginatedViewModel<Message>
                {
                    Items = pagedMessages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };

                // 날짜 범위를 ViewBag에 저장 (폼에 표시하기 위함)
                ViewBag.StartDate = actualStartDate.ToString("yyyy-MM-dd");
                ViewBag.EndDate = actualEndDate.ToString("yyyy-MM-dd");

                _logger.LogInformation("메시지 목록 조회 완료: 총 {TotalItems}개 중 {PagedCount}개 표시",
                    totalItems, pagedMessages.Count);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 목록 조회 중 오류 발생: {ErrorMessage}", ex.Message);

                // 오류 발생 시 빈 모델로 뷰 표시
                TempData["ErrorMessage"] = "메시지 목록을 불러오는 중 오류가 발생했습니다.";
                return View(new PaginatedViewModel<Message>());
            }
        }


        /// <summary>
        /// 메시지 상세 정보 페이지를 표시합니다.
        /// </summary>
        /// <param name="id">조회할 메시지 ID</param>
        /// <returns>메시지 상세 뷰 또는 NotFound</returns>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 메시지 ID: {Id}", id);
                    return BadRequest("유효하지 않은 메시지 ID입니다.");
                }

                _logger.LogInformation("메시지 상세 조회: ID={Id}", id);

                var message = await _messageService.GetMessageByIdAsync(id);

                if (message == null)
                {
                    _logger.LogWarning("메시지를 찾을 수 없음: ID={Id}", id);
                    return NotFound($"ID가 {id}인 메시지를 찾을 수 없습니다.");
                }

                return View(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 상세 조회 중 오류 발생: ID={Id}, 오류={ErrorMessage}", id, ex.Message);

                // 오류 메시지를 TempData에 저장하여 뷰에서 표시
                TempData["ErrorMessage"] = $"메시지 상세 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}";

                // 이전 페이지로 리디렉션
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 특정 그룹에 속한 메시지 목록을 표시합니다.
        /// </summary>
        /// <param name="groupId">조회할 그룹 ID</param>
        /// <returns>메시지 목록 뷰</returns>
        public async Task<IActionResult> ByGroup(int groupId)
        {
            try
            {
                if (groupId <= 0)
                {
                    _logger.LogWarning("잘못된 그룹 ID: {GroupId}", groupId);
                    return BadRequest("유효하지 않은 그룹 ID입니다.");
                }

                _logger.LogInformation("그룹별 메시지 조회: GroupID={GroupId}", groupId);

                var messages = await _messageService.GetMessagesByGroupIdAsync(groupId);

                // 결과가 null인 경우 빈 목록으로 대체 (방어적 프로그래밍)
                if (messages == null)
                {
                    _logger.LogWarning("메시지 서비스에서 null 반환됨. 빈 목록으로 대체합니다.");
                    messages = new List<Message>();
                }

                // 그룹 정보를 ViewBag에 추가
                ViewBag.FilterType = "그룹";
                ViewBag.FilterId = groupId;

                return View("Index", messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "그룹별 메시지 조회 중 오류 발생: GroupID={GroupId}, 오류={ErrorMessage}", groupId, ex.Message);

                // 오류 메시지를 TempData에 저장하여 뷰에서 표시
                TempData["ErrorMessage"] = $"그룹 메시지를 불러오는 중 오류가 발생했습니다: {ex.Message}";

                // 빈 목록 반환
                return View("Index", new List<Message>());
            }
        }

        /// <summary>
        /// 특정 알람 포인트와 관련된 메시지 목록을 표시합니다.
        /// </summary>
        /// <param name="alarmPointId">조회할 알람 포인트 ID</param>
        /// <returns>메시지 목록 뷰</returns>
        public async Task<IActionResult> ByAlarmPoint(int alarmPointId)
        {
            try
            {
                if (alarmPointId <= 0)
                {
                    _logger.LogWarning("잘못된 알람 포인트 ID: {AlarmPointId}", alarmPointId);
                    return BadRequest("유효하지 않은 알람 포인트 ID입니다.");
                }

                _logger.LogInformation("알람 포인트별 메시지 조회: AlarmPointID={AlarmPointId}", alarmPointId);

                var messages = await _messageService.GetMessagesByAlarmPointIdAsync(alarmPointId);

                // 결과가 null인 경우 빈 목록으로 대체 (방어적 프로그래밍)
                if (messages == null)
                {
                    _logger.LogWarning("메시지 서비스에서 null 반환됨. 빈 목록으로 대체합니다.");
                    messages = new List<Message>();
                }

                // 알람 포인트 정보를 ViewBag에 추가
                ViewBag.FilterType = "알람 포인트";
                ViewBag.FilterId = alarmPointId;

                return View("Index", messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "알람 포인트별 메시지 조회 중 오류 발생: AlarmPointID={AlarmPointId}, 오류={ErrorMessage}", alarmPointId, ex.Message);

                // 오류 메시지를 TempData에 저장하여 뷰에서 표시
                TempData["ErrorMessage"] = $"알람 포인트 메시지를 불러오는 중 오류가 발생했습니다: {ex.Message}";

                // 빈 목록 반환
                return View("Index", new List<Message>());
            }
        }
        /// <summary>
        /// 메시지 상태 업데이트 액션
        /// </summary>
        /// <param name="id">업데이트할 메시지 ID</param>
        /// <param name="status">새 상태 값</param>
        /// <returns>메시지 상세 페이지로 리디렉션</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("잘못된 메시지 ID: {Id}", id);
                    return BadRequest("유효하지 않은 메시지 ID입니다.");
                }

                if (string.IsNullOrEmpty(status))
                {
                    _logger.LogWarning("상태 값이 비어 있음: 메시지 ID={Id}", id);
                    return BadRequest("상태 값은 필수입니다.");
                }

                _logger.LogInformation("메시지 상태 업데이트: ID={Id}, Status={Status}", id, status);

                var result = await _messageService.UpdateMessageStatusAsync(id, status);

                if (result)
                {
                    TempData["SuccessMessage"] = "메시지 상태가 성공적으로 업데이트되었습니다.";
                }
                else
                {
                    TempData["ErrorMessage"] = "메시지 상태 업데이트에 실패했습니다.";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "메시지 상태 업데이트 중 오류 발생: ID={Id}, Status={Status}, 오류={ErrorMessage}",
                    id, status, ex.Message);

                TempData["ErrorMessage"] = $"메시지 상태 업데이트 중 오류가 발생했습니다: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}