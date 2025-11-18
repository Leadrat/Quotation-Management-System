using System;
using System.Collections.Generic;
using System.Text.Json;
using AutoMapper;
using CRM.Application.Clients.Dtos;
using CRM.Domain.Entities;

namespace CRM.Application.Mapping
{
    public class ClientHistoryProfile : Profile
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ClientHistoryProfile()
        {
            CreateMap<ClientHistory, ClientHistoryEntryDto>()
                .ForMember(dest => dest.ActorDisplayName, opt => opt.MapFrom(src => ResolveActorName(src)))
                .ForMember(dest => dest.ChangedFields, opt => opt.MapFrom(src => (IReadOnlyList<string>)(src.ChangedFields ?? new List<string>())))
                .ForMember(dest => dest.BeforeSnapshot, opt => opt.MapFrom(src => DeserializeSnapshot(src.BeforeSnapshot)))
                .ForMember(dest => dest.AfterSnapshot, opt => opt.MapFrom(src => DeserializeSnapshot(src.AfterSnapshot)))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => DeserializeMetadata(src.Metadata)));

            CreateMap<SuspiciousActivityFlag, SuspiciousActivityDto>()
                .ForMember(dest => dest.Reasons, opt => opt.MapFrom(src => (IReadOnlyList<string>)(src.Reasons ?? new List<string>())))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => DeserializeMetadata(src.Metadata)));

            CreateMap<Client, ClientTimelineSummaryDto>()
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.TotalChangeCount, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletionReason, opt => opt.Ignore())
                .ForMember(dest => dest.RestorationWindowExpiresAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.DeletedAt != null))
                .ForMember(dest => dest.LatestEntry, opt => opt.Ignore());
        }

        private static string ResolveActorName(ClientHistory history)
        {
            if (history.ActorUser == null)
            {
                return "System";
            }

            var first = history.ActorUser.FirstName ?? string.Empty;
            var last = history.ActorUser.LastName ?? string.Empty;
            var full = $"{first} {last}".Trim();
            return string.IsNullOrWhiteSpace(full) ? history.ActorUser.Email : full;
        }

        private static object? DeserializeSnapshot(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, SerializerOptions);
            }
            catch
            {
                return null;
            }
        }

        private static HistoryMetadataDto DeserializeMetadata(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new HistoryMetadataDto();
            }

            try
            {
                return JsonSerializer.Deserialize<HistoryMetadataDto>(json, SerializerOptions) ?? new HistoryMetadataDto();
            }
            catch
            {
                return new HistoryMetadataDto();
            }
        }
    }
}

