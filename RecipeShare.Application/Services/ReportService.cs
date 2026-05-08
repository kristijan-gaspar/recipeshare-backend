using Mapster;
using RecipeShare.Application.Common;
using RecipeShare.Application.DTOs.Reports;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;
using RecipeShare.Domain.Enums;

namespace RecipeShare.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IRecipeRepository _recipeRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IAdminRecipeService _adminRecipeService;
        private readonly IAdminCommentService _adminCommentService;
        private readonly IAdminUserService _adminUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public ReportService(
            IReportRepository reportRepository,
            IRecipeRepository recipeRepository,
            ICommentRepository commentRepository,
            IAdminRecipeService adminRecipeService,
            IAdminCommentService adminCommentService,
            IAdminUserService adminUserService,
            INotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
            _reportRepository = reportRepository;
            _recipeRepository = recipeRepository;
            _commentRepository = commentRepository;
            _adminRecipeService = adminRecipeService;
            _adminCommentService = adminCommentService;
            _adminUserService = adminUserService;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;

        }

        public async Task CreateAsync(int reporterId, CreateReportRequest request)
        {
            // Validate target exists and get reported user id
            int reportedUserId;

            if (request.TargetType == ReportTargetType.Recipe)
            {
                var recipe = await _recipeRepository.GetByIdAsync(request.TargetId);
                if (recipe == null || recipe.IsDeleted)
                    throw new NotFoundException("Recipe not found.");
                reportedUserId = recipe.UserId;
            }
            else if (request.TargetType == ReportTargetType.Comment)
            {
                var comment = await _commentRepository.GetByIdAsync(request.TargetId);
                if (comment == null || comment.IsDeleted)
                    throw new NotFoundException("Comment not found.");
                reportedUserId = comment.UserId;
            }
            else
            {
                throw new BadRequestException("Invalid target type.");
            }

            if (reportedUserId == reporterId)
                throw new BadRequestException("You cannot report your own content.");

            var exists = await _reportRepository.ExistsAsync(reporterId, request.TargetType, request.TargetId);
            if (exists)
                throw new BadRequestException("You have already reported this content.");

            var report = new Report
            {
                TargetType = request.TargetType,
                TargetId = request.TargetId,
                ReportedUserId = reportedUserId,
                ReporterId = reporterId,
                Reason = request.Reason,
                Description = request.Description
            };

            await _reportRepository.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PagedResponse<ReportResponse>> GetReportsAsync( ReportStatus? status, int page,int pageSize)
        {
            var reports = await _reportRepository.GetFilteredAsync(status, page, pageSize);
            var totalCount = await _reportRepository.CountFilteredAsync(status);

            var items = reports.Adapt<List<ReportResponse>>();

            return new PagedResponse<ReportResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                HasNextPage = page * pageSize < totalCount
            };
        }

        public async Task<ReportDetailResponse> GetByIdAsync(int reportId)
        {
            var report = await _reportRepository.GetByIdWithDetailsAsync(reportId);
            if (report == null)
                throw new NotFoundException("Report not found.");

            string? targetContent = null;

            if (report.TargetType == ReportTargetType.Recipe)
            {
                var recipe = await _recipeRepository.GetByIdAsync(report.TargetId);
                targetContent = recipe?.Title;
            }
            else if (report.TargetType == ReportTargetType.Comment)
            {
                var comment = await _commentRepository.GetByIdAsync(report.TargetId);
                targetContent = comment?.Content;
            }

            var response = report.Adapt<ReportDetailResponse>();
            response.TargetContent = targetContent;

            return response;
        }

        public async Task ResolveAsync(int reportId, int adminId, ResolveReportRequest request)
        {
            var report = await _reportRepository.GetByIdWithDetailsAsync(reportId);
            if (report == null)
                throw new NotFoundException("Report not found.");
            if (report.Status != ReportStatus.Pending)
                throw new BadRequestException("Report is already processed.");

           
            if (request.ContentAction == AdminAction.SoftDelete)
            {
                if (report.TargetType == ReportTargetType.Recipe)
                    await _adminRecipeService.SoftDeleteAsync(report.TargetId);
                else if (report.TargetType == ReportTargetType.Comment)
                    await _adminCommentService.SoftDeleteAsync(report.TargetId);
            }

            
            switch (request.UserAction)
            {
                case AdminAction.Warning:
                    await _notificationService.SendWarningNotificationAsync(report.ReportedUserId, report.Reason);
                    break;
                case AdminAction.Block:
                    await _adminUserService.BlockAsync(report.ReportedUserId);
                    break;
                case AdminAction.SoftDelete:
                    await _adminUserService.SoftDeleteAsync(report.ReportedUserId);
                    break;
            }


            report.Status = ReportStatus.Resolved;
            report.ContentAction = request.ContentAction;
            report.UserAction = request.UserAction;
            report.AdminNote = request.AdminNote;
            report.ResolvedByAdminId = adminId;
            report.ResolvedAt = DateTime.UtcNow;

            _reportRepository.Update(report);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DismissAsync(int reportId, int adminId)
        {
            var report = await _reportRepository.GetByIdAsync(reportId);
            if (report == null)
                throw new NotFoundException("Report not found.");
            if (report.Status != ReportStatus.Pending)
                throw new BadRequestException("Report is already processed.");

            report.Status = ReportStatus.Dismissed;
            report.ResolvedByAdminId = adminId;
            report.ResolvedAt = DateTime.UtcNow;

            _reportRepository.Update(report);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
