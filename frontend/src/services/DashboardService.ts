import { BaseService } from "./BaseService";
import { DashboardData } from "@/types/api/Dashboard";

class DashboardService extends BaseService {
    constructor() {
        super("dashboard");
    }

    async getData(): Promise<DashboardData> {
        return await this.get<DashboardData>("");
    }
}

export default new DashboardService();
