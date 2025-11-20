import { useMemo } from "react";
import type { ChartData, ChartSeries } from "@/types/reports";

interface ChartTransformOptions {
  chartType: "line" | "bar" | "pie" | "funnel" | "heatmap";
  data: any[];
  xKey?: string;
  yKey?: string;
  seriesKey?: string;
}

export function useCharts(options: ChartTransformOptions) {
  const { chartType, data, xKey = "date", yKey = "value", seriesKey } = options;

  const chartData = useMemo<ChartData | null>(() => {
    if (!data || data.length === 0) return null;

    switch (chartType) {
      case "line":
      case "bar": {
        const categories = data.map((item) => item[xKey] || String(item[xKey]));
        const series: ChartSeries[] = seriesKey
          ? [
              {
                name: seriesKey,
                data: data.map((item) => Number(item[yKey]) || 0),
              },
            ]
          : [
              {
                name: "Value",
                data: data.map((item) => Number(item[yKey]) || 0),
              },
            ];

        return {
          chartType,
          title: "",
          series,
          categories,
        };
      }

      case "pie": {
        const categories = data.map((item) => item[xKey] || String(item[xKey]));
        const series: ChartSeries[] = [
          {
            name: "Distribution",
            data: data.map((item) => Number(item[yKey]) || 0),
          },
        ];

        return {
          chartType: "pie",
          title: "",
          series,
          categories,
        };
      }

      case "funnel": {
        const categories = data.map((item) => item[xKey] || String(item[xKey]));
        const series: ChartSeries[] = [
          {
            name: "Count",
            data: data.map((item) => Number(item[yKey] || item.count) || 0),
          },
        ];

        return {
          chartType: "funnel",
          title: "",
          series,
          categories,
        };
      }

      default:
        return null;
    }
  }, [chartType, data, xKey, yKey, seriesKey]);

  return { chartData };
}

