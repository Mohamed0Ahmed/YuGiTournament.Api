﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.RegularExpressions;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data.Configurations
{
      public class PlayerConfiguration : IEntityTypeConfiguration<Player>
      {
            public void Configure(EntityTypeBuilder<Player> player)
            {
                  player.HasKey(p => p.PlayerId);

                  player.Property(p => p.PlayerId)
                        .ValueGeneratedOnAdd()
                        .UseIdentityColumn();


                  player.Property(p => p.FullName)
                        .IsRequired()
                        .HasMaxLength(100);

                  player.Property(p => p.MatchesPlayed)
                        .HasDefaultValue(0);


                  player.Property(p => p.WinRate)
                        .HasDefaultValue(0);

                  player.Property(p => p.Rank)
                        .IsRequired();

                  player.Property(p => p.Wins)
                        .HasDefaultValue(0);

                  player.Property(p => p.Losses)
                        .HasDefaultValue(0);

                  player.Property(p => p.Draws)
                        .HasDefaultValue(0);

                  player.Property(p => p.Points)
                  .HasDefaultValue(0);

                  player.Property(m => m.IsDeleted)
                      .IsRequired();
                  player.Property(m => m.LeagueNumber)
                       .IsRequired();
                  player.Property(p => p.GroupNumber)
                        .IsRequired(false);
            }
      }
}
