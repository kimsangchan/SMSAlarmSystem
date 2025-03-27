using Microsoft.AspNetCore.Mvc;
using SMSAlarmSystem.Services.Services;
using System;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Controllers
{
    public class MessagesController : Controller
    {
        private readonly MessageService _messageService;

        public MessagesController(MessageService messageService)
        {
            _messageService = messageService;
        }

        // 메시지 목록 페이지
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            // 기본 날짜 범위 설정 (오늘부터 7일 전까지)
            startDate ??= DateTime.Now.AddDays(-7);
            endDate ??= DateTime.Now;

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            var messages = await _messageService.GetMessagesByDateRangeAsync(startDate.Value, endDate.Value);
            return View(messages);
        }

        // 메시지 상세 페이지
        public async Task<IActionResult> Details(int id)
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            return View(message);
        }

        // 그룹별 메시지 조회
        public async Task<IActionResult> ByGroup(int groupId)
        {
            var messages = await _messageService.GetMessagesByGroupIdAsync(groupId);
            return View("Index", messages);
        }

        // 알람 포인트별 메시지 조회
        public async Task<IActionResult> ByAlarmPoint(int alarmPointId)
        {
            var messages = await _messageService.GetMessagesByAlarmPointIdAsync(alarmPointId);
            return View("Index", messages);
        }
    }
}
