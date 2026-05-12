using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.Api.Application.Contracts;
using HotelListing.Api.Application.DTOs.Booking;
using HotelListing.Api.Common.Constants;
using HotelListing.Api.Common.Models.Extensions;
using HotelListing.Api.Common.Models.Paging;
using HotelListing.Api.Common.Results;
using HotelListing.Api.Domain;
using HotelListing.Api.Domain.Enums;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;

namespace HotelListing.Api.Application.Services;

public class BookingService(HotelListingDbContext context, IUserService usersService, IMapper mapper) : IBookingService
{
    public async Task<Result<PagedResult<GetBookingDto>>> GetBookingsForHotelAsync(int hotelId, PaginationParameters paginationParameters)
    {
        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);
        if (!hotelExists)
            return Result<PagedResult<GetBookingDto>>.Failure(new Error(ErrorCodes.NotFound, $"Hotel '{hotelId}' was not found."));

        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId)
            .OrderBy(b => b.CheckIn)
            .ProjectTo<GetBookingDto>(mapper.ConfigurationProvider)
            .ToPagedResultAsync(paginationParameters);

        return Result<PagedResult<GetBookingDto>>.Success(bookings);
    }

    public async Task<Result<PagedResult<GetBookingDto>>> GetUserBookingsForHotelAsync(int hotelId, PaginationParameters paginationParameters)
    {
        var userId = usersService.UserId;

        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);
        if (!hotelExists)
            return Result<PagedResult<GetBookingDto>>.Failure(new Error(ErrorCodes.NotFound, $"Hotel '{hotelId}' was not found."));

        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId && b.UserdId == userId)
            .OrderBy(b => b.CheckIn)
            .ProjectTo<GetBookingDto>(mapper.ConfigurationProvider)
            .ToPagedResultAsync(paginationParameters);


        return Result<PagedResult<GetBookingDto>>.Success(bookings);
    }

    public async Task<Result<GetBookingDto>> CreateBookingAsync(CreateBookingDto dto)
    {
        var userId = usersService.UserId;
        bool overlaps = await IsOverlap(dto.HotelId, userId, dto.CheckIn, dto.CheckOut);

        if (overlaps)
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Conflict, "The selected dates overlap with an existing booking."));

        var hotel = await context.Hotels
            .Where(h => h.Id == dto.HotelId)
            .FirstOrDefaultAsync();

        if (hotel is null)
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.NotFound, $"Hotel '{dto.HotelId}' was not found."));

        var nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;
        var totalPrice = hotel.PerNightRate * nights;
        var booking = mapper.Map<Booking>(dto);
        booking.UserdId = userId;

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var result = mapper.Map<GetBookingDto>(booking);

        return Result<GetBookingDto>.Success(result);
    }

    public async Task<Result<GetBookingDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto dto)
    {
        var userId = usersService.UserId;

        bool overlaps = await IsOverlap(hotelId, userId, dto.CheckIn, dto.CheckOut, bookingId);

        if (overlaps)
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Conflict, "The selected dates overlap with an existing booking."));

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.HotelId == hotelId
                && b.UserdId == userId);

        if (booking is null)
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found."));

        if (booking.Status == BookingStatus.Cancelled)
            return Result<GetBookingDto>.Failure(new Error(ErrorCodes.Conflict, "Cancelled bookings cannot be modified."));

        mapper.Map(dto, booking);
        var perNight = booking.Hotel!.PerNightRate;
        var nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;
        booking.TotalPrice = perNight * nights;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        var updated = mapper.Map<GetBookingDto>(booking);

        return Result<GetBookingDto>.Success(updated);
    }

    private async Task<bool> IsOverlap(int hotelId, string userId, DateOnly checkIn, DateOnly checkOut, int? bookingId = null)
    {
        var query = context.Bookings
            .Where(
                    b => b.HotelId == hotelId
                    && b.Status != BookingStatus.Cancelled
                    && checkIn < b.CheckOut
                    && checkOut > b.CheckIn
                    && b.UserdId == userId)
            .AsQueryable();

        if (bookingId.HasValue)
        {
            query = query.Where(q => q.Id != bookingId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<Result> CancelBookingAsync(int hotelId, int bookingId)
    {
        var userId = usersService.UserId;

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.HotelId == hotelId
                && b.UserdId == userId);

        if (booking is null)
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found."));

        if (booking.Status == BookingStatus.Cancelled)
            return Result.Failure(new Error(ErrorCodes.Conflict, "This booking has already been cancelled."));

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> AdminCancelBookingAsync(int hotelId, int bookingId)
    {
        var userId = usersService.UserId;

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.HotelId == hotelId);

        if (booking is null)
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found."));

        if (booking.Status == BookingStatus.Cancelled)
            return Result.Failure(new Error(ErrorCodes.Conflict, "This booking has already been cancelled."));

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> AdminConfirmBookingAsync(int hotelId, int bookingId)
    {
        var userId = usersService.UserId;

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b =>
                b.Id == bookingId
                && b.HotelId == hotelId);

        if (booking is null)
            return Result.Failure(new Error(ErrorCodes.NotFound, $"Booking '{bookingId}' was not found."));

        if (booking.Status == BookingStatus.Cancelled)
            return Result.Failure(new Error(ErrorCodes.Conflict, "This booking has already been cancelled."));

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return Result.Success();
    }
}