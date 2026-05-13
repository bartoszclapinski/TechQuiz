using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Categories;

public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
