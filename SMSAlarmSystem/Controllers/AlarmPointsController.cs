using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMSAlarmSystem.Core.Models;
using SMSAlarmSystem.Services.Services;
using System.Threading.Tasks;

namespace SMSAlarmSystem.Controllers
{
    public class AlarmPointsController : Controller
    {
        private readonly AlarmService _alarmService;
        private readonly MessageGroupService _messageGroupService;

        public AlarmPointsController(AlarmService alarmService, MessageGroupService messageGroupService)
        {
            _alarmService = alarmService;
            _messageGroupService = messageGroupService;
        }

        // 알람 포인트 목록 페이지
        public async Task<IActionResult> Index()
        {
            // AlarmService에 GetAllAlarmPointsAsync 메서드를 추가해야 합니다
            var alarmPoints = await _alarmService.GetAllAlarmPointsAsync();
            return View(alarmPoints);
        }

        // 알람 포인트 상세 페이지
        public async Task<IActionResult> Details(int id)
        {
            // AlarmService에 GetAlarmPointByIdAsync 메서드를 추가해야 합니다
            var alarmPoint = await _alarmService.GetAlarmPointByIdAsync(id);
            if (alarmPoint == null)
            {
                return NotFound();
            }
            return View(alarmPoint);
        }

        // 알람 포인트 생성 페이지 (GET)
        public async Task<IActionResult> Create()
        {
            var groups = await _messageGroupService.GetActiveGroupsAsync();
            ViewBag.MessageGroupId = new SelectList(groups, "Id", "Name");
            return View();
        }

        // 알람 포인트 생성 처리 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlarmPoint alarmPoint)
        {
            if (ModelState.IsValid)
            {
                await _alarmService.AddAlarmPointAsync(alarmPoint);
                return RedirectToAction(nameof(Index));
            }

            var groups = await _messageGroupService.GetActiveGroupsAsync();
            ViewBag.MessageGroupId = new SelectList(groups, "Id", "Name", alarmPoint.MessageGroupId);
            return View(alarmPoint);
        }

        // 알람 포인트 수정 페이지 (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var alarmPoint = await _alarmService.GetAlarmPointByIdAsync(id);
            if (alarmPoint == null)
            {
                return NotFound();
            }

            var groups = await _messageGroupService.GetActiveGroupsAsync();
            ViewBag.MessageGroupId = new SelectList(groups, "Id", "Name", alarmPoint.MessageGroupId);
            return View(alarmPoint);
        }

        // 알람 포인트 수정 처리 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AlarmPoint alarmPoint)
        {
            if (id != alarmPoint.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _alarmService.UpdateAlarmPointAsync(alarmPoint);
                return RedirectToAction(nameof(Index));
            }

            var groups = await _messageGroupService.GetActiveGroupsAsync();
            ViewBag.MessageGroupId = new SelectList(groups, "Id", "Name", alarmPoint.MessageGroupId);
            return View(alarmPoint);
        }

        // 알람 포인트 삭제 페이지 (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var alarmPoint = await _alarmService.GetAlarmPointByIdAsync(id);
            if (alarmPoint == null)
            {
                return NotFound();
            }
            return View(alarmPoint);
        }

        // 알람 포인트 삭제 처리 (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _alarmService.DeleteAlarmPointAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // 알람 트리거 페이지 (GET)
        public async Task<IActionResult> Trigger(int id)
        {
            var alarmPoint = await _alarmService.GetAlarmPointByIdAsync(id);
            if (alarmPoint == null)
            {
                return NotFound();
            }
            return View(alarmPoint);
        }

        // 알람 트리거 처리 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Trigger(int id, string messageContent)
        {
            await _alarmService.TriggerAlarmAsync(id, messageContent);
            return RedirectToAction(nameof(Index));
        }
    }
}
